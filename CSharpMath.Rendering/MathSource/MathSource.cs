using CSharpMath.Atoms;
using CSharpMath.Interfaces;

namespace CSharpMath.Rendering {
  public readonly struct MathSource {
    public MathSource(string latex) {
      var res = MathLists.BuildResultFromString(latex);
      LaTeX = latex;
      MathList = res.MathList;
      Error = res.Error;
    }

    public MathSource(IMathList mathList) {
      LaTeX = MathListBuilder.MathListToString(mathList);
      MathList = mathList;
      Error = null;
    }

    public IMathList MathList { get; }
    public string LaTeX { get; }
    public string Error { get; }
  }
}
