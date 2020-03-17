using System.Linq;

namespace CSharpMath.DevUtils.TypographyTest {
  static class GlyphLayout {
    public static Typography.TextLayout.GlyphLayout Get =>
      new Typography.TextLayout.GlyphLayout {
        Typeface =
          new Typography.OpenFont.OpenFontReader()
          .Read(new System.IO.FileStream(
            System.IO.Directory.EnumerateFiles(Paths.ReferenceFontsFolder, "*.otf").First(),
            System.IO.FileMode.Open))
    };
  }
}
