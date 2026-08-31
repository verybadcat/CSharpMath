param(
    [Parameter(Mandatory = $true)][string]$ModeMathRoot,
    [Parameter(Mandatory = $true)][string]$PackageDirectory,
    [string]$Output = "$PSScriptRoot\raw-results.ndjson"
)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'ModeMathStudyBench.fsproj'
$modeMathCommit = '1ba976e0125fa03f41d6fe4f5aa4c60df89cfb10'
$csharpMathCommit = 'e1cb9d6270289ec1903022ba7a3275ba52a3d819'
$modeMathVersion = '0.12.0-csharpmath-study.1ba976e'
$modeMathPackageFile = "ModeMath.$modeMathVersion.nupkg"
$modeMathPackageSha256 = '0B03977732F2CE13EC9A290DD28A30DAFEF48E81ED36FC0B38670B4342700926'
$modeMathGit = Join-Path $ModeMathRoot '.git'
if (-not (Test-Path $modeMathGit)) { throw "ModeMathRoot is not a Git checkout: $ModeMathRoot" }
$modeMathRevision = (git -C $ModeMathRoot rev-parse HEAD).Trim()
if ($modeMathRevision -ne $modeMathCommit) { throw "ModeMathRoot must be pinned to $modeMathCommit (found $modeMathRevision)" }
if ((git -C $ModeMathRoot status --porcelain).Trim()) { throw 'ModeMathRoot must have a clean working tree' }
$csharpRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$csharpRevision = (git -C $csharpRoot rev-parse HEAD).Trim()
if ($csharpRevision -ne $csharpMathCommit) { throw "CSharpMath checkout must be pinned to $csharpMathCommit (found $csharpRevision)" }
git -C $csharpRoot diff --quiet HEAD -- CSharpMath CSharpMath.Rendering CSharpMath.SkiaSharp research/ModeMathStudyBench/ModeMathStudyBench.fsproj research/ModeMathStudyBench/Program.fs research/ModeMathStudyBench/capture.ps1 research/ModeMathStudyBench/aggregate.ps1
if ($LASTEXITCODE -ne 0) { throw 'CSharpMath product/harness sources must be clean' }
git -C $csharpRoot diff --cached --quiet HEAD -- CSharpMath CSharpMath.Rendering CSharpMath.SkiaSharp research/ModeMathStudyBench/ModeMathStudyBench.fsproj research/ModeMathStudyBench/Program.fs research/ModeMathStudyBench/capture.ps1 research/ModeMathStudyBench/aggregate.ps1
if ($LASTEXITCODE -ne 0) { throw 'CSharpMath product/harness sources must be clean in the index' }
$package = Get-Item (Join-Path $PackageDirectory $modeMathPackageFile) -ErrorAction SilentlyContinue
if ($null -eq $package) { throw "Missing pinned ModeMath package: $modeMathPackageFile" }
$packageHash = (Get-FileHash $package.FullName -Algorithm SHA256).Hash
if ($packageHash -ne $modeMathPackageSha256) { throw "Pinned package hash mismatch: expected $modeMathPackageSha256, found $packageHash" }
$restorePackagesPath = Join-Path ([IO.Path]::GetTempPath()) ("csharpmath-modemath-restore-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $restorePackagesPath | Out-Null
try {
    # Use a fresh package cache so an existing global cache cannot satisfy this study.
    $configPath = Join-Path $restorePackagesPath 'NuGet.Config'
    $localSource = [System.Security.SecurityElement]::Escape((Resolve-Path $PackageDirectory).Path)
    $config = '<configuration><packageSources><clear /><add key="study-package" value="' + $localSource + '" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources></configuration>'
    Set-Content -LiteralPath $configPath -Encoding utf8 -Value $config
    & dotnet restore $project --configfile $configPath --no-cache --force-evaluate -p:RestorePackagesPath=$restorePackagesPath -p:NoWarn=NU1605
    if ($LASTEXITCODE -ne 0) { throw 'Harness restore failed' }
    $assetsPath = Join-Path $PSScriptRoot 'obj\project.assets.json'
    $assets = Get-Content $assetsPath -Raw | ConvertFrom-Json
    $assetLibraries = @($assets.libraries.psobject.Properties.Name)
    if ($assetLibraries -notcontains "ModeMath/$modeMathVersion") { throw "Resolved assets do not contain pinned ModeMath/$modeMathVersion" }
    $restoredPackage = Get-Item (Join-Path $restorePackagesPath "modemath\$modeMathVersion\$modeMathPackageFile") -ErrorAction SilentlyContinue
    if ($null -eq $restoredPackage) { throw 'Pinned package was not restored into the isolated package cache' }
    if ((Get-FileHash $restoredPackage.FullName -Algorithm SHA256).Hash -ne $modeMathPackageSha256) { throw 'Restored package hash does not match validated source package' }
    & dotnet build $project -c Release --no-restore -p:RestorePackagesPath=$restorePackagesPath -p:GeneratePackageOnBuild=false
    if ($LASTEXITCODE -ne 0) { throw 'Harness Release build failed' }
$runtime = ((& dotnet --list-runtimes | Select-String 'Microsoft.NETCore.App 10\.').Line -split ' ')[1]
$meta = [ordered]@{ record = 'metadata'; modeMathRevision = $modeMathRevision; csharpMathRevision = $csharpRevision; os = [Environment]::OSVersion.VersionString; runtime = $runtime; sdk = (& dotnet --version).Trim(); rid = 'win-x64'; configuration = 'Release'; font = 'ModeMath Layout 32 px / CSharpMath FontSize 24 pt at 96 DPI'; corpus = @('x^2 + y_1', '\frac{-b \pm \sqrt{b^2 - 4ac}}{2a}', '\int_0^1 x^2\,dx'); iterations = @{ parse = 10000; layout = 5000; draw = 2000; editor = 2000 }; packageFile = $package.Name; packageSha256 = $packageHash }
Set-Content -Path $Output -Value ($meta | ConvertTo-Json -Compress) -Encoding utf8
function Invoke-Capture($library, $mode, $sample) {
    $lines = & dotnet run --project $project -c Release --no-build -- $mode 2>&1
    if ($LASTEXITCODE -ne 0) { throw "capture failed: $library $mode" }
    $expectedNames = if ($mode -like 'cold-*') { @("$library-cold-render") } else { @("$library-parse-batch", "$library-layout-batch", "$library-draw-batch", "$library-editor-x2") }
    $rows = @()
    foreach ($line in $lines) {
        if (-not [string]::IsNullOrWhiteSpace($line) -and $line -notmatch '^\s*\{') { throw "Unexpected non-JSON output from $library $mode" }
        if ($line -match '^\{') {
            $row = $line | ConvertFrom-Json
            $rows += $row
            $row | Add-Member -NotePropertyName record -NotePropertyValue 'metric'
            $row | Add-Member -NotePropertyName library -NotePropertyValue $library
            $row | Add-Member -NotePropertyName mode -NotePropertyValue $mode
            $row | Add-Member -NotePropertyName sample -NotePropertyValue $sample
            ($row | ConvertTo-Json -Compress) | Add-Content -Path $Output -Encoding utf8
        }
    }
    if ($rows.Count -ne $expectedNames.Count) { throw "Expected $($expectedNames.Count) metric row(s) from $library $mode; found $($rows.Count)" }
    $actualNames = @($rows | ForEach-Object { [string]$_.name } | Sort-Object -Unique)
    if ((Compare-Object $actualNames ($expectedNames | Sort-Object -Unique))) { throw "Unexpected metric names from $library $mode" }
}
1..10 | ForEach-Object { $i = $_; Invoke-Capture 'modemath' 'cold-modemath' $i; Invoke-Capture 'csharpmath' 'cold-csharpmath' $i }
1..3 | ForEach-Object { $i = $_; Invoke-Capture 'modemath' 'steady-modemath' $i; Invoke-Capture 'csharpmath' 'steady-csharpmath' $i }
    Write-Host "Captured $Output"
}
finally {
    if (Test-Path $restorePackagesPath) { Remove-Item -LiteralPath $restorePackagesPath -Recurse -Force }
}
