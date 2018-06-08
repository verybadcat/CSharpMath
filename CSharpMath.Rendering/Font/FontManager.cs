using System;
using System.Diagnostics;
using System.IO;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public static class FontManager {
    static FontManager() {
      var bytes = Resources.Otf;
      var reader = new OpenFontReader();
      LatinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      LatinMathTypeface.UpdateAllCffGlyphBounds();
    }

    public const string LatinMathFontName = "latinmodern-math";
    public static Typeface LatinMathTypeface { get; }

    public static MathFont LatinMath(float pointSize) {
      return new MathFont(LatinMathFontName, LatinMathTypeface, pointSize);
    }
  }
}
