namespace CSharpMath.Apple {
  public static class AppleFontManager {
    public const string LatinMathFontName = "latinmodern-math";
    public static CoreGraphics.CGFont LatinMathCG { get; } =
      new System.Func<CoreGraphics.CGFont>(() => {
        using var fontDataProvider = new CoreGraphics.CGDataProvider(
          Foundation.NSData.FromStream(
            new Resources.ManifestResourceProvider(
              System.Reflection.Assembly.GetExecutingAssembly()
            ).ManifestStream(LatinMathFontName + ".otf")
          )
        );
        return CoreGraphics.CGFont.CreateFromProvider(fontDataProvider);
      })();
    public static AppleMathFont LatinMath(float pointSize) =>
      new AppleMathFont(LatinMathFontName, LatinMathCG, pointSize);
  }
}
