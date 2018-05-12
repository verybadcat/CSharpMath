using P = System.IO.Path;

namespace CSharpMath.DevUtils {
  class Global {
    /// <summary>
    /// The path of the global CSharpMath folder
    /// </summary>
    public static readonly string Path = ((System.Func<string>)(() => {
      var L = typeof(Global).Assembly.Location;
      while (P.GetFileName(L) != nameof(CSharpMath)) L = P.GetDirectoryName(L);
      return L;
    }))();
  }
}
