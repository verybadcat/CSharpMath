using CSharpMath.Atoms;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public readonly struct MathSource : ISource {
    public MathSource(string latex) {
      (MathList, ErrorMessage) = MathLists.BuildResultFromString(latex);
      LaTeX = latex;
    }

    public MathSource(IMathList mathList) {
      LaTeX = MathListBuilder.MathListToString(mathList);
      MathList = mathList;
      ErrorMessage = null;
    }

    public IMathList MathList { get; }
    public string LaTeX { get; }
    public string ErrorMessage { get; }
    public bool IsValid => MathList != null;
  }
}
