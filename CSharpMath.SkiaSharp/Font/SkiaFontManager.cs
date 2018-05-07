using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public static class SkiaFontManager {
    static SkiaFontManager() {
      var bytes = SkiaResources.Otf;
      var reader = new OpenFontReader();
      _latinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      _latinMathSKTypeface = SKTypeface.FromStream(new MemoryStream(bytes, false));
    }

    public const string LatinMathFontName = "latinmodern-math";

    private static Typeface _latinMathTypeface;
    public static Typeface LatinMathTypeface {
      get => _latinMathTypeface;
    }
    private static SKTypeface _latinMathSKTypeface;
    public static SKTypeface LatinMathSKTypeface {
      get => _latinMathSKTypeface;
    }

    public static SkiaMathFont LatinMath(float pointSize) {
      return new SkiaMathFont(LatinMathFontName, LatinMathTypeface, LatinMathSKTypeface, pointSize);
    }
  }
}
