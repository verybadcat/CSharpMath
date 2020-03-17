using CSharpMath.Atom;

namespace CSharpMath.Rendering {
  public class MathSource : ISource, System.IEquatable<MathSource> {
    public static MathSource FromLaTeX(string latex) {
      var (mathList, errorMessage) = LaTeXBuilder.TryMathListFromLaTeX(latex);
      return new MathSource(mathList) { ErrorMessage = errorMessage, _latex = latex };
    }
    public MathSource(MathList mathList) => MathList = mathList;
    public MathList MathList { get; }
    private string _latex;
    public string LaTeX => _latex ??= LaTeXBuilder.MathListToLaTeX(MathList);
    public string ErrorMessage { get; private set; }
    public bool IsValid => MathList != null;
    public override int GetHashCode() => unchecked(MathList.GetHashCode() * 2519);
    public override bool Equals(object obj) => obj is MathSource s ? Equals(s) : false;
    public bool Equals(MathSource other) => // CSharpMath.Forms bindings rely on this
      MathList.NullCheckingStructuralEquality(other.MathList) && ErrorMessage == other.ErrorMessage;
  }
}
