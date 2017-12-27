using System;
using System.Reflection;
using CSharpMath.Resources;

namespace CSharpMath.Ios.Resources
{
  public static class IosResourceProviders
  {
    public static ManifestResourceProvider Manifest() {
      var assembly = Assembly.GetExecutingAssembly();
      return new ManifestResourceProvider(assembly);
    }
  }
}
