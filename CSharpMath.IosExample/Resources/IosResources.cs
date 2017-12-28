
using CSharpMath.Resources;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Ios.Resources
{
  public static class IosResources
  {
    private static JToken _latinMath;
    internal static string LatinMathContent
    {
      get
      {
        var provider = IosResourceProviders.Manifest();
        var content = provider.ManifestString("latinmodern-math.json");
        return content;
      }
    }
    public static JToken LatinMath
    {
      get
      {
        if (_latinMath == null)
        {
          var content = LatinMathContent;
          _latinMath = JTokenLoader.FromString(content);
        }
        return _latinMath;
      }
    }
  }
}
