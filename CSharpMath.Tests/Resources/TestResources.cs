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
    internal static string LatinMathContent =>
      TestResourceProviders.Provider.ManifestString("latinmodern-math.json");
    public static JToken LatinMath =>
      _latinMath ?? (_latinMath = JTokenLoader.FromString(LatinMathContent));
  }
}
