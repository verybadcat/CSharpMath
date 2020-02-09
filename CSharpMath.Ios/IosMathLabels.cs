namespace CSharpMath.Ios {
  using Apple;
  public static class IosMathLabels {
    public static AppleMathView MathView(string latex, float fontSize) =>
      new AppleMathView(AppleTypesetters.LatinMath, fontSize) { LaTeX = latex };
  }
}