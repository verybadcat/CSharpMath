#define CODE_ANALYSIS
using static IgnoreTypographyWarnings;
using Suppress = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

[assembly:Suppress("Style", "IDE0017:Simplify object initialization", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "IDE0018:Inline variable declaration", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "IDE0019:Use pattern matching", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "IDE0044:Add readonly modifier", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "CS0162:Unreachable code detected", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "CS0168:Variable declared but never used", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "CS0219:Variable assigned but never used", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("Style", "CSM001:Culture-invariant ToString()", Target = _Typography, Justification = Justification, Scope = Scope)]

[assembly: Suppress("Style", "CS0162:Unreachable code detected", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly: Suppress("Style", "CS0168:Variable declared but never used", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]

[System.Diagnostics.DebuggerStepThrough]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
static class IgnoreTypographyWarnings {
  public const string Justification =
    "These messages are from the Typography library." +
    "It does not use apply these optimizations because" +
    "it has to support Visual Studio 2010.";
  public const string Scope = "namespaceanddescendants";
  public const string _Typography = "~N:Typography";
  public const string _Poly2Tri = "~N:Poly2Tri";
}