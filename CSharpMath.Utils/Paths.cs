using P = System.IO.Path;

namespace CSharpMath.DevUtils {
  class Paths {
    /// <summary>
    /// The path of the global CSharpMath folder
    /// </summary>
    public static readonly string Global = ((System.Func<string>)(() => {
      var L = typeof(Paths).Assembly.Location;
      while (P.GetFileName(L) != nameof(CSharpMath)) L = P.GetDirectoryName(L);
      return L;
    }))();

    /// <summary>
    /// The path of the latinmodern-math.otf file
    /// </summary>
    public static readonly string LatinModernMath = P.Combine(Global, nameof(CSharpMath) + "." + nameof(SkiaSharp), "Font Reference", "latinmodern-math.otf");
    /// <summary>
    /// The path of the Otf.cs file
    /// </summary>
    public static readonly string OtfFile = P.Combine(Global, nameof(CSharpMath) + "." + nameof(SkiaSharp), "Font Reference", "Otf.cs");
  }
}
