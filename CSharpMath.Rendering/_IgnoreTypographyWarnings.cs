#define CODE_ANALYSIS
using static _IgnoreTypographyWarnings;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0018:Inline variable declaration", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CS0162:Unreachable code detected", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CS0168", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CS0169", Justification = Justification, Scope = Scope, Target = Target)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CS0414", Justification = Justification, Scope = Scope, Target = Target)]

[System.Diagnostics.DebuggerStepThrough, System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
static class _IgnoreTypographyWarnings {
  public const string Justification = "These messages are from the Typography library. It does not use apply these optimizations because it has to support Visual Studio 2010.";
  public const string Scope = "namespace";
  public const string Target = "~N:Typography";
}