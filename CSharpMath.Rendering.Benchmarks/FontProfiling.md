# Font profiling artifact

Run `dotnet run -c Release -p CSharpMath.Rendering.Benchmarks -- --font-profile`. Use
`--font-profile-samples`, `--font-profile-iterations`, and `--font-profile-batches` to control work,
or `CSHARP_MATH_FONT_PROFILE_OUTPUT` / `--font-profile-output` for the output path. Invoke
`--font-profile-smoke` with or without `--font-profile`; it is a cheap deterministic correctness
check for CI.

The artifact reports raw samples for direct per-resource SFNT parsing, separate first-math and
first-mixed cold workers, global rendering, and alternating formula/font batches. It records bytes
allocated by the managed runtime and bytes retained after a forced GC; process working set is not
used as managed retention. The three embedded faces are loaded by identity and hashed from their
actual bytes. The tracked Comic Neue asset is a real runtime `LocalTypeface`. Table tags are parsed
from each SFNT directory; CFF/glyf and hint/instruction, MATH, GSUB, GPOS, and GDEF presence can be
read directly from `tables`. The `packaging` object reports the measured bundled-font, custom-asset,
rendering-assembly, and generated-font-data fields for the current build. `exercisedPaths`,
`switchingSequence`, and `globalFaces` identify operations actually performed; each `globalFaces`
entry includes the cmap-validated corpus, LocalTypeface usage, and successful draw evidence.
Parse-only table inventory is not presented as outline/cache execution evidence.

Compare distributions and batch retained-memory trends, not thresholds: OS, runtime, Skia, and
process startup noise affect measurements. The artifact helps investigate font startup and cache
growth discussed by issues #107 and #191; it does not prove causality or impose a performance gate.

Captured smoke result (Release, .NET 10 on the development Windows host): 10 raw samples (three
parse-only faces, three rendered bundled-face scenarios, first math, first mixed text, global
alternation, and custom Comic Neue), four font inventories, and zero worker or schema errors.
