namespace CSharpMath.Tests.FrontEnd.Resources;

public static class ManifestResources {
  static readonly System.Lazy<string> _latinMath = new System.Lazy<string>(() => {
    var assembly = typeof(ManifestResources).Assembly;
    using var stream = assembly.GetManifestResourceStream("CSharpMath.Tests.FrontEnd.Resources.latinmodern-math.json");
    if (stream == null) throw new System.InvalidOperationException("Could not find embedded resource latinmodern-math.json");
    using var reader = new System.IO.StreamReader(stream);
    return reader.ReadToEnd();
  });
  
  public static string LatinMath => _latinMath.Value;
}
