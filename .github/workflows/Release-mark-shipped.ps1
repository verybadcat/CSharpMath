# From https://github.com/dotnet/winforms/pull/4837/changes

[CmdletBinding(PositionalBinding=$false)]
param ()

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function MarkShipped([string]$dir) {
    $shippedFilePath = Join-Path $dir "PublicAPI.Shipped.txt"
    $shipped = @(Get-Content $shippedFilePath -Encoding UTF8)

    $unshippedFilePath = Join-Path $dir "PublicAPI.Unshipped.txt"
    $unshipped = @(Get-Content $unshippedFilePath -Encoding UTF8)
    $removed = @()
    $removedPrefix = "*REMOVED*";
    Write-Host "Processing $dir"

    foreach ($item in $unshipped) {
        if ($item.Length -gt 0) {
            if (($item -match '^#nullable\s') -and $shipped.Contains($item)) {
                continue
            }
            if ($item.StartsWith($removedPrefix)) {
                $item = $item.Substring($removedPrefix.Length)
                $removed += $item
            }
            else {
                $shipped += $item
            }
        }
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [string[]]$filteredShipped = @($shipped | ?{ -not $removed.Contains($_) })
    [System.IO.File]::WriteAllLines($shippedFilePath, $filteredShipped, $encoding)
    [System.IO.File]::WriteAllLines($unshippedFilePath, [string[]]@(), $encoding)
}

try {
    Push-Location $PSScriptRoot\..\..

    foreach ($file in Get-ChildItem -re -in "PublicAPI.Shipped.txt") {
        $dir = Split-Path -parent $file
        MarkShipped $dir
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    exit 1
}
finally {
    Pop-Location
}
