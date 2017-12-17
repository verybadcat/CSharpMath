using CSharpMath.Apple;

namespace CSharpMath.Ios
{
  static class IosMathLabels
  {
    public static AppleLatexView LatexView(string latex)
    {
      var typesettingContext = AppleTypesetters.CreateLatinMath();
      var view = new AppleLatexView(typesettingContext);
      view.SetLatex(latex);
      return view;
    }
  }
}