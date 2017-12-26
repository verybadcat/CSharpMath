using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Resources;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Tests.Resources {
  public static class TestResources {
    private static JToken _latinMath;
    internal static string LatinMathContent {
      get {
        var provider = TestResourceProviders.Provider;
        var content = provider.ManifestString("latinmodern-math.json");
        return content;
      }
    }
    public static JToken LatinMath {
      get {
        if (_latinMath == null) {
          var content = LatinMathContent;
          _latinMath = JTokenLoader.FromString(content);
        }
        return _latinMath;
      }
    }
  }
}
