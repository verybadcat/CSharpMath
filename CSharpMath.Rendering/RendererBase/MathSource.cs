using CSharpMath.Atoms;

namespace CSharpMath.Rendering.Renderer {
  public readonly struct MathSource : ISource {
    public MathSource(string latex) {
      (MathList, ErrorMessage) = LaTeXBuilder.TryMathListFromLaTeX(latex);
      LaTeX = latex;
    }
    public MathSource(MathList mathList) {
      LaTeX = LaTeXBuilder.MathListToLaTeX(mathList);
      MathList = mathList;
      ErrorMessage = null;
    }
    public MathList MathList { get; }
    public string LaTeX { get; }
    public string ErrorMessage { get; }
    public bool IsValid => MathList != null;
  }
}
