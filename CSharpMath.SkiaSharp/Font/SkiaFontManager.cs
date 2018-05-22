using System;
using System.Diagnostics;
using System.IO;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public static class SkiaFontManager {
    static SkiaFontManager() {
      var bytes = SkiaResources.Otf;
      var reader = new OpenFontReader();
      LatinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      LatinMathTypeface.UpdateAllCffGlyphBounds();
    }

    public const string LatinMathFontName = "latinmodern-math";
    public static Typeface LatinMathTypeface { get; }

    public static SkiaMathFont LatinMath(float pointSize) {
      return new SkiaMathFont(LatinMathFontName, LatinMathTypeface, pointSize);
    }
  }
}
