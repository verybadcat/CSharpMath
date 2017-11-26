
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Resources {
  public static class ResourceLoader {
    private const string _latinModernJsonFilename = "latinmodern-math.json";
    private static string _resourcesPath
      => Path.Combine(Directory.GetCurrentDirectory(), "Resources");
    static ResourceLoader() {
      var content = ResourceProvider.ManifestString(_latinModernJsonFilename);
      var jObject = JsonConvert.DeserializeObject(content);
      LatinMath = (jObject as JObject).Root;
      var type = jObject.GetType();
    }
    public static JToken LatinMath;


  }
}
