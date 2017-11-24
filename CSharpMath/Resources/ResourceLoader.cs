using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.IO;

namespace CSharpMath.Resources {
  public static class ResourceLoader {

    private static string _resourcesPath
      => Path.Combine(Assembly.GetExecutingAssembly().Location, "Resources");
    static ResourceLoader() {
      var builder = new ConfigurationBuilder();
      builder.SetBasePath(_resourcesPath);
      builder.AddJsonFile("latinmodern-math.json");
      var configuration = builder.Build();
      LatinMathConfiguration = configuration;
    }
    public static IConfigurationRoot LatinMathConfiguration;


  }
}
