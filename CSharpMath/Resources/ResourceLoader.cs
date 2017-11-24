using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using System.Dynamic;

namespace CSharpMath.Resources {
  public static class ResourceLoader {
    private const string _latinModernJsonFilename = "latinmodern-math.json";
    private static string _resourcesPath
      => Path.Combine(Directory.GetCurrentDirectory(), "Resources");
    static ResourceLoader() {
      var content = ResourceProvider.ManifestString(_latinModernJsonFilename);
      var expando = JsonConvert.DeserializeObject<ExpandoObject>(content);
      LatinMath = expando;
    }
    public static ExpandoObject LatinMath;


  }
}
