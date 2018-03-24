using CSharpMath.Apple;

namespace CSharpMath.Ios
{
  public static class IosMathLabels
  {
    public static AppleLatexView LatexView(string latex, float fontSize)
    {
      var typesettingContext = AppleTypesetters.LatinMath;
      var view = new AppleLatexView(typesettingContext, fontSize);
      view.SetLatex(latex);
      return view;
    }
  }
}