using CSharpMath.Apple;

namespace CSharpMath.Ios
{
  static class IosMathLabels
  {
    public static AppleLatexView LatexView(string latex)
    {
      var view = new AppleLatexView();
      view.SetLatex(latex);
      return view;
    }
  }
}