# ModeMath architecture study (issue #273)

## Decision

ModeMath is a useful source of focused techniques, not a replacement architecture for CSharpMath. We should adapt selected ideas behind existing compatibility and editor invariants, and land them incrementally behind the relevant work items.

## Reproducibility

The comparison used CSharpMath revision `e1cb9d62` (full commit `e1cb9d6270289ec1903022ba7a3275ba52a3d819`, origin/master) and ModeMath revision `1ba976e0125fa03f41d6fe4f5aa4c60df89cfb10`, on Windows `10.0.19045`, `win-x64`, .NET SDK `10.0.302` and runtime `10.0.10`. The ModeMath library build was clean. The capture workflow requires both source checkouts to be at these commits; it permits only the untracked study artifacts in the CSharpMath worktree and rejects tracked product/harness changes. The solution test-project restore has an `NU1605` dependency caveat; with `-p:NoWarn=NU1605`, ModeMath tests were `630/630` and CSharpMath Core tests were `1515/1515`.

Representative commands (use local variables for paths):

```powershell
$ModeMathRoot = 'C:\path\to\ModeMath-1ba976e'
$ModeMathPackages = 'C:\path\to\ModeMath-packages'
dotnet build "$ModeMathRoot\ModeMath\ModeMath.fsproj" -c Release
dotnet test "$ModeMathRoot\ModeMath.slnx" -c Release -p:NoWarn=NU1605
dotnet test CSharpMath.Core.Tests\CSharpMath.Core.Tests.csproj -c Release -p:NoWarn=NU1605
dotnet restore research\ModeMathStudyBench\ModeMathStudyBench.fsproj --source $ModeMathPackages --source https://api.nuget.org/v3/index.json -p:NoWarn=NU1605
dotnet build research\ModeMathStudyBench\ModeMathStudyBench.fsproj -c Release --no-restore -p:GeneratePackageOnBuild=false
```

The study-only package is `ModeMath.0.12.0-csharpmath-study.1ba976e.nupkg`; verify its SHA-256 as `0B03977732F2CE13EC9A290DD28A30DAFEF48E81ED36FC0B38670B4342700926` before restore. The capture script enforces the exact filename, hash, package version, clean source checkout, and pinned source revision, then restores/builds the Release harness from that local package before `--no-build` capture. The unique prerelease version prevents a silent fallback to public `0.12.0`.

## Feature matrix and decisions

| Area | ModeMath observation | CSharpMath decision |
|---|---|---|
| Parsing | Small explicit parser; rejects unknown input | Study/adapt where it preserves public behavior |
| LaTeX roundtrip | `Read`/`Write`, canonical braces | Prototype against compatibility corpus |
| Layout/rendering | `Layout` then `Painter`, generated metrics | Adapt techniques; retain backend contracts |
| Tentative brackets | Editor-state tentative delimiters | Adapt incrementally under #82 |
| Navigation | Cursor over immutable MA tree | Reject wholesale; adapt internal invariants/transactions |
| Decorated editing | Limited editor model | Keep separate prototype; caret/empty-slot work under #158 |
| Undo/redo | Transaction-oriented editor state | Adapt transaction ideas after invariant tests |
| Typed functions/radicals | Reversible, longest-token recognition | Prototype under #83; reject substring/opaque semantics |
| Public compatibility | New API and narrower accepted input | Preserve CSharpMath public API and accepted behavior |

Generated `MathFontData` is the prototype hybrid default path under #191/#293; preserve the runtime `LocalTypeface` fallback. This keeps generated data useful without making embedded data the only deployment path.

## Complexity and size

The reproducible source-only count is 4,914 ModeMath runtime lines: 4,133 handwritten lines in the ten `ModeMath/*.fs` files other than `MathFontData.g.fs`, plus 781 generated lines in `MathFontData.g.fs`. Its `tools/MathTableGen/*.fs` generator is 658 lines and `tools/trimfont.py` is 104 lines. The count is physical lines from tracked files, including blank/comment lines. Exact PowerShell counting form (run from each repository root) is:

```powershell
$paths = git ls-files 'ModeMath/*.fs' # use the corresponding glob for each tree
$lines = 0; foreach ($path in $paths) { $lines += (git show "HEAD:$path" | Measure-Object).Count }
"$($paths.Count) files, $lines lines"
```

It excludes project files, tests, examples, build output, and all other directories. For context, the same command with `CSharpMath/**/*.cs`/`CSharpMath/*.cs` gives CSharpMath 84 tracked files and 7,862 lines; with `CSharpMath.Rendering/**/*.cs`/`CSharpMath.Rendering/*.cs` it gives Rendering 27 files and 2,040 lines (tests and build output excluded). File/line counts are not an engineering-effort or quality measure.

## Font and package evidence

Raw embedded faces measured 87,324 B for ModeMath versus 786,744 B for CSharpMath: 699,420 B (88.9%) less for ModeMath. This is not package-size or runtime-memory proof and excludes dependency/feature differences. ModeMath package was 251,207 B and `ModeMath.dll` 458,752 B. CSharpMath's compressed chain was `117,571 + 834,847 + 22,512 ~= 974,930 B`; its DLLs were `242,688 + 1,798,144 + 11,264 B`. These are not apples-to-apples: dependencies, platforms, APIs, and supported features differ.

## Benchmark evidence

The normalized three-formula harness aggregate is checked in at [results-captured.md](../research/ModeMathStudyBench/results-captured.md), computed from 10 fresh cold samples and 3 steady samples per library. Cold medians were ModeMath 229.681 ms (179.948-401.301) versus CSharpMath 513.767 ms (367.005-737.039); managed allocation 267,800 B versus 22,053,456 B; retained 90,192 B versus 3,896,944 B. Steady per-formula parse was 10.117 versus 27.631 us and 2.833 versus 5.703 KB allocation; layout 18.030 versus 69.908 us and 6.682 versus 31.749 KB; draw 35.735 versus 182.400 us and 1.586 versus 10.094 KB. A three-key editor sequence was 22.666 versus 39.297 us and 6.758 versus 25.307 KB. Per-metric ranges are in the checked-in aggregate. Results remain directional/manual, managed-only, use different editor histories/timers, and are not a performance gate.

These numbers are directional/manual, managed-only, use different editor histories and timers, and omit Skia native allocation, JIT/code pages, process startup, and platform variance. They provide no performance gate.

## Licensing inventory

ModeMath source is MIT (Summatic). Its Latin Modern-derived face is under the GUST Font License/LPPL-derived obligations, including derived-font renaming; AMS faces carry OFL notices and reserved-name restrictions. Generated metrics and outlines retain font provenance. Before shipping any adapted/generated asset, CSharpMath needs a third-party-notices and package audit. This inventory is not legal advice.

## Unknowns and next experiments

Unknowns include broad LaTeX compatibility, accessibility/selection semantics, malformed-input recovery, font fallback coverage, trimming/AOT behavior, native memory, and cross-platform rendering parity. Next experiments should: (1) differential-test parser and roundtrip behavior against the existing corpus; (2) prototype #82/#83/#158/#191/#293 behind focused tests; (3) measure AOT/trimming and native memory on supported platforms; (4) audit licenses and generated-file provenance; and (5) compare editor histories with a shared scripted workload. None should become a gate until methodology and feature parity are agreed.
