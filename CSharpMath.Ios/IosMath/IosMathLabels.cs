using CSharpMath.Apple;

namespace CSharpMath.Ios
{
  static class IosMathLabels
  {
    public static AppleLatexView LatexView(string latex, float fontSize)
    {
      var typesettingContext = AppleTypesetters.LatinMath;
      var view = new AppleLatexView(typesettingContext, 15);
      view.SetLatex(latex);
      return view;
    }
  }
}