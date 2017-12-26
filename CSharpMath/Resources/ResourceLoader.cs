using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Resources {
  public class JTokenLoader {
    public JTokenLoader(string path) {
      _resourcesPath = path;
    }
    private const string _latinModernJsonFilename = "latinmodern-math.json";

    public JTokenLoader() {
      var content = ManifestResourceProvider.ManifestString(_latinModernJsonFilename);
      var jObject = JsonConvert.DeserializeObject(content);
      LatinMath = (jObject as JObject).Root;
      var type = jObject.GetType();
    }
    public JToken LatinMath;
  }
}
