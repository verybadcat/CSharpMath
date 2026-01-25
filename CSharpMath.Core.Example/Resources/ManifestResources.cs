namespace CSharpMath.Core;

public static class ManifestResources {
  static readonly System.Lazy<Newtonsoft.Json.Linq.JToken> _latinMath = new System.Lazy<Newtonsoft.Json.Linq.JToken>(() => {
    var assembly = typeof(ManifestResources).Assembly;
    using var stream = assembly.GetManifestResourceStream("CSharpMath.Core.Example.Resources.latinmodern-math.json");
    if (stream == null) throw new System.InvalidOperationException("Could not find embedded resource latinmodern-math.json");
    using var reader = new System.IO.StreamReader(stream);
    return Newtonsoft.Json.Linq.JToken.Parse(reader.ReadToEnd());
  });
  
  public static Newtonsoft.Json.Linq.JToken LatinMath => _latinMath.Value;
}
