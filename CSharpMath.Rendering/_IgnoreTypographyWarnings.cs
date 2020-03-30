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

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
static class IgnoreTypographyWarnings {
  public const string Justification = "These messages are from the Typography library. Its code is not very clean.";
  public const string Scope = "namespaceanddescendants";
  public const string _Typography = nameof(Typography);
}