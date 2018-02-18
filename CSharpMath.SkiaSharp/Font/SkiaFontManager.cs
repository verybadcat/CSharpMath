using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;
using Typography.OpenFont;

namespace CSharpMath.SkiaSharp {
  public static class SkiaFontManager
  {
    private static bool _initialized = false;
    public static bool Initialized => _initialized;
    /// <summary>
    /// Initializes the CSharpMath.SkiaSharp library with a stream to the Latin Modern Math font.
    /// The given stream is not referenced anymore; dispose it yourself.
    /// </summary>
    /// <param name="latinModernMath">The stream to the Latin Modern Math font.</param>
    /// <exception cref="InvalidOperationException">Thrown when called more than once.</exception>
    public static void Initialize(Stream latinModernMath) {
      Debug.WriteLine($"Trying to initialize {nameof(CSharpMath)}.{nameof(SkiaSharp)}...");
      if (!_initialized) {
        byte[] bytes;
        using (var memStream = new MemoryStream()) {
          latinModernMath.CopyTo(memStream);
          bytes = memStream.ToArray();
        }

        var reader = new OpenFontReader();
        _latinMathTypeface = reader.Read(new MemoryStream(bytes, false));
        _latinMathSKTypeface = SKTypeface.FromStream(new MemoryStream(bytes, false));
        _initialized = true;
        Debug.WriteLine($"Initialization successful.");
      }
      else throw new InvalidOperationException("You are initializing this library more than once. It is probably a bug.");
    }

    public const string LatinMathFontName = "latinmodern-math";

    private static Exception UninitializedException =>
        new InvalidOperationException($"You must call {nameof(CSharpMath)}.{nameof(SkiaSharp)}." +
          $"{nameof(SkiaFontManager)}.{nameof(Initialize)} with a stream pointing to an OTF file" +
          $" with the Latin Modern Math font prior to using other methods of this library.");

    private static Typeface _latinMathTypeface;
    public static Typeface LatinMathTypeface {
      get => _latinMathTypeface ?? throw UninitializedException;
    }
    private static SKTypeface _latinMathSKTypeface;
    public static SKTypeface LatinMathSKTypeface {
      get => _latinMathSKTypeface ?? throw UninitializedException;
    }

    public static SkiaMathFont LatinMath(float pointSize) {
      return new SkiaMathFont(LatinMathFontName, LatinMathTypeface, LatinMathSKTypeface, pointSize);
    }
  }
}
