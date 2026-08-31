# ModeMath comparison harness

This research-only harness supports the architecture study in issue #273. It is deliberately not a performance gate: it reports raw wall time and managed allocation/retention for the same three-formula corpus on both libraries. The checked-in aggregate in `results-captured.md` is a captured result, not a rerun or a statistically complete sample archive.

Set these local-only variables to your checkout locations (do not commit machine-specific paths):

```powershell
$ModeMathRoot = 'C:\path\to\ModeMath-1ba976e'
$ModeMathPackages = 'C:\path\to\ModeMath-packages'
$ModeMathVersion = '0.12.0-csharpmath-study.1ba976e'
git clone https://github.com/SummaticLtd/ModeMath.git $ModeMathRoot
git -C $ModeMathRoot checkout --detach 1ba976e0125fa03f41d6fe4f5aa4c60df89cfb10
dotnet build "$ModeMathRoot\ModeMath\ModeMath.fsproj" -c Release
dotnet pack "$ModeMathRoot\ModeMath\ModeMath.fsproj" -c Release --no-build -p:PackageVersion=$ModeMathVersion -o $ModeMathPackages
Get-FileHash "$ModeMathPackages\ModeMath.$ModeMathVersion.nupkg" -Algorithm SHA256
```

Restore and build this harness from the CSharpMath repository. The local package source must come first so the pinned package is selected; keep nuget.org for transitive dependencies:

```powershell
dotnet restore research\ModeMathStudyBench\ModeMathStudyBench.fsproj `
  --source $ModeMathPackages `
  --source "https://api.nuget.org/v3/index.json" `
  --no-cache --force-evaluate -p:NoWarn=NU1605
dotnet build research\ModeMathStudyBench\ModeMathStudyBench.fsproj `
  -c Release --no-restore -p:GeneratePackageOnBuild=false
```

`NU1605` is suppressed only for this pinned study: the current `SimpleTests` dependency requests a newer `FSharp.Core` than ModeMath's pinned floor. The ModeMath library itself builds without that suppression.

Each cold sample must be a fresh process. The checked-in `capture.ps1` validates both pinned revisions and the clean tracked product/harness source state, then restores into a fresh isolated package cache from the exact local package and builds the Release harness before capture. It alternates ten cold samples per library and captures three steady samples per library. It preserves every JSON metric line in NDJSON with sample, library, mode, revisions, environment, and package hash metadata. Run it:

```powershell
powershell -ExecutionPolicy Bypass -File research\ModeMathStudyBench\capture.ps1 `
  -ModeMathRoot $ModeMathRoot -PackageDirectory $ModeMathPackages
```

Run steady-state parse, layout, draw, and three-key editor updates separately:

```powershell
dotnet run --project research\ModeMathStudyBench -c Release --no-build -- steady-modemath
dotnet run --project research\ModeMathStudyBench -c Release --no-build -- steady-csharpmath
```

The harness intentionally measures managed allocations only. Skia native allocations, JIT/code pages, process startup, and platform variance are outside these numbers and must be stated alongside any result.

Each command writes one JSON object per metric to stdout. `capture.ps1` saves stdout as `raw-results.ndjson`; the aggregate table in `results-captured.md` is computed from that file. `RetainedBytes` is a post-GC process measurement and is especially sensitive to runtime/library initialization. The ModeMath 32 px em matches CSharpMath's 24 pt at 96 DPI (approximately 32 px); keep this equivalence when comparing runs.
