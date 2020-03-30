using Newtonsoft.Json.Linq;
using System.IO;

namespace CSharpMath.Resources {
  public static class ManifestResources {
    private static JToken? _latinMath;
    internal static Stream LatinMathContent =>
      new ManifestResourceProvider(System.Reflection.Assembly.GetExecutingAssembly())
      .ManifestStream("latinmodern-math.json")
      ?? throw new Structures.InvalidCodePathException("Failed to load Latin Modern Math");
    public static JToken LatinMath {
      get {
        if (_latinMath is null) {
          using var textReader = new StreamReader(LatinMathContent);
          using var reader = new Newtonsoft.Json.JsonTextReader(textReader);
          _latinMath =
            new Newtonsoft.Json.JsonSerializer().Deserialize<JObject>(reader).Root;
        }
        return _latinMath;
      }
    }
  }
}