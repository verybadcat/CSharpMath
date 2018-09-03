using System.Linq;

namespace CSharpMath.DevUtils.TypographyTest {
  static class _Statics {
    public static Typography.TextLayout.GlyphLayout GlyphLayout => new Typography.TextLayout.GlyphLayout() {
      Typeface = new Typography.OpenFont.OpenFontReader().Read(new System.IO.FileStream(System.IO.Directory.EnumerateFiles(Paths.FontReferenceFolder, "*.otf").First(), System.IO.FileMode.Open))
    };
  }
}
