using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Resources {
  public static class JTokenLoader {

    public static JToken FromString(string content) {
      var jObject = JsonConvert.DeserializeObject(content);
      return (jObject as JObject).Root;
    }
  }
}
