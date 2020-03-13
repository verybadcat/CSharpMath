#define CODE_ANALYSIS
using static IgnoreTypographyWarnings;
using Suppress = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

[assembly:Suppress("", "IDE0017:Object initialization can be simplified", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0018:Variable declaration can be inlined", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0019:Use pattern matching", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0031:Null check can be simplified", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0044:Make field readonly", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0051:Private member is unused", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0052:Private member can be removed as the value assigned to it is never read", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0056:Indexing can be simplified", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0059:Unnecessary assignment of a value to a variable", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0060:Remove unused parameter", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0063:'using' statement can be simplified", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0066:Use 'switch' expression", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE1006:Naming rule violation: Prefix '_' is not expected", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0162:Unreachable code detected", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0168:Variable is declared but never used", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0219:Variable is assigned but never used", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0649:Field is never assigned to and will always have its default value", Target = _Typography, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CSM001:Culture-invariant ToString()", Target = _Typography, Justification = Justification, Scope = Scope)]

[assembly:Suppress("", "IDE0017:Object initialization can be simplified", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0018:Variable declaration can be inlined", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0019:Use pattern matching", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0044:Make field readonly", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0051:Private member is unused", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0052:Private member can be removed as the value assigned to it is never read", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0059:Unnecessary assignment of a value to a variable", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0060:Remove unused parameter", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "IDE0066:Use 'switch' expression", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0162:Unreachable code detected", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]
[assembly:Suppress("", "CS0168:Variable declared but never used", Target = _Poly2Tri, Justification = Justification, Scope = Scope)]

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