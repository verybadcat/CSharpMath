using CSharpMath.Apple;

namespace CSharpMath.Ios
{
  public static class IosMathLabels
  {
    public static AppleMathView MathView(string latex, float fontSize)
    {
      var typesettingContext = AppleTypesetters.LatinMath;
      var view = new AppleMathView(typesettingContext, fontSize);
      view.SetLatex(latex);
      return view;
    }
  }
}