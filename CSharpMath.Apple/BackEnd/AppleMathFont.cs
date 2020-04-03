using CoreGraphics;
using CoreText;

namespace CSharpMath.Apple {
  /// <remarks>Corresponds to MTFont in iosMath.</remarks>
  public struct AppleMathFont : Display.FrontEnd.IFont<ushort> {
    public float PointSize { get; }
    public CGFont CgFont { get; private set; }
    public CTFont CtFont { get; private set; }
    public string Name { get; private set; }
    internal AppleMathFont(string name, CGFont cgFont, float size) {
      PointSize = size;
      Name = name;
      CgFont = cgFont;
      CtFont = new CTFont(CgFont, size, CGAffineTransform.MakeIdentity());
    }
    public AppleMathFont(AppleMathFont cloneMe, float pointSize) {
      PointSize = pointSize;
      Name = cloneMe.Name;
      CgFont = cloneMe.CgFont;
      CtFont = new CTFont(CgFont, pointSize, CGAffineTransform.MakeIdentity());
    }
    private const string LatinMathFontName = "latinmodern-math";
    static AppleMathFont() {
      using var fontDataProvider = new CGDataProvider(
        Foundation.NSData.FromStream(
          new Resources.ManifestResourceProvider(
            System.Reflection.Assembly.GetExecutingAssembly()
          ).ManifestStream(LatinMathFontName + ".otf")
        )
      );
      LatinMathCG = CGFont.CreateFromProvider(fontDataProvider);
    }
    public static CGFont LatinMathCG { get; }
    public static AppleMathFont LatinMath(float pointSize) =>
      new AppleMathFont(LatinMathFontName, LatinMathCG, pointSize);
  }
}
