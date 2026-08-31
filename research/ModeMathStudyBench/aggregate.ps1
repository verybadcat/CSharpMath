param(
    [string]$InputPath = "$PSScriptRoot\raw-results.ndjson",
    [string]$Output = "$PSScriptRoot\aggregates.json",
    [switch]$Check
)
$ErrorActionPreference = 'Stop'
$rows = Get-Content $InputPath | ForEach-Object { $_ | ConvertFrom-Json } | Where-Object record -eq 'metric'
$expectedLibraries = @('modemath', 'csharpmath')
$expectedSteadySuffixes = @('parse-batch', 'layout-batch', 'draw-batch', 'editor-x2')
if ($rows.Count -ne 44) { throw "Expected exactly 44 canonical metric rows; found $($rows.Count)" }
foreach ($library in $expectedLibraries) {
    $cold = @($rows | Where-Object { $_.library -eq $library -and $_.mode -eq "cold-$library" })
    $coldName = "$library-cold-render"
    $coldSamples = @($cold | ForEach-Object { [int]$_.sample } | Sort-Object)
    if ($cold.Count -ne 10 -or (($cold | Select-Object -ExpandProperty name -Unique) -join ',') -ne $coldName -or (($coldSamples -join ',') -ne ((1..10) -join ','))) {
        throw "Expected exactly 10 rows for $library cold capture"
    }
    $steady = @($rows | Where-Object { $_.library -eq $library -and $_.mode -eq "steady-$library" })
    $steadyNames = @($steady | Select-Object -ExpandProperty name -Unique)
    $expectedNames = @($expectedSteadySuffixes | ForEach-Object { "$library-$_" } | Sort-Object)
    if ($steady.Count -ne 12 -or (($steadyNames | Sort-Object) -join ',') -ne ($expectedNames -join ',')) {
        throw "Expected four distinct steady metrics, three rows each, for $library"
    }
    foreach ($name in $expectedNames) {
        $metric = @($steady | Where-Object name -eq $name)
        $samples = @($metric | ForEach-Object { [int]$_.sample } | Sort-Object)
        if ($metric.Count -ne 3 -or (($samples -join ',') -ne ((1..3) -join ','))) { throw "Expected samples 1..3 exactly once for $name" }
    }
}
$unexpected = @($rows | Where-Object { $_.library -notin $expectedLibraries -or $_.mode -notin @("cold-$($_.library)", "steady-$($_.library)") })
if ($unexpected.Count -ne 0) { throw 'Unexpected metric group identity in raw results' }
$formulaCount = 3
$groups = $rows | Group-Object library, name
$result = foreach ($group in $groups) {
    $sample = $group.Group
    $isCold = $sample[0].name -like '*cold*'
    $denominator = if ($isCold) { 1 } elseif ($sample[0].name -like '*editor*') { $sample[0].iterations } else { $sample[0].iterations * $formulaCount }
    $times = @($sample | ForEach-Object { [double]$_.elapsedNanoseconds / 1000.0 / $denominator }) | Sort-Object
    $alloc = @($sample | ForEach-Object { [double]$_.allocatedBytes / $denominator }) | Sort-Object
    $retained = @($sample | ForEach-Object { [double]$_.retainedBytes }) | Sort-Object
    $middle = [math]::Floor($times.Count / 2)
    $timeMedian = if ($times.Count % 2) { $times[$middle] } else { ($times[$middle - 1] + $times[$middle]) / 2 }
    $middle = [math]::Floor($alloc.Count / 2)
    $allocMedian = if ($alloc.Count % 2) { $alloc[$middle] } else { ($alloc[$middle - 1] + $alloc[$middle]) / 2 }
    $middle = [math]::Floor($retained.Count / 2)
    $retainedMedian = if ($retained.Count % 2) { $retained[$middle] } else { ($retained[$middle - 1] + $retained[$middle]) / 2 }
    [ordered]@{ library = $sample[0].library; metric = $sample[0].name; samples = $sample.Count; elapsedUsMedian = $timeMedian; elapsedUsMin = $times[0]; elapsedUsMax = $times[-1]; allocatedBytesMedian = $allocMedian; allocatedBytesMin = $alloc[0]; allocatedBytesMax = $alloc[-1]; retainedBytesMedian = $retainedMedian; retainedBytesMin = $retained[0]; retainedBytesMax = $retained[-1] }
}
$json = @($result) | ConvertTo-Json -Depth 4
if ($Check) {
    if (-not (Test-Path $Output)) { throw "Missing aggregate artifact: $Output" }
    $expected = @(Get-Content $Output -Raw | ConvertFrom-Json)
    if (($json | ConvertFrom-Json | ConvertTo-Json -Depth 4) -ne ($expected | ConvertTo-Json -Depth 4)) { throw 'Aggregate artifact does not match raw-results.ndjson' }
    Write-Host 'Aggregate check passed.'
} else {
    Set-Content -Path $Output -Value $json -Encoding utf8
    Write-Output $json
}
