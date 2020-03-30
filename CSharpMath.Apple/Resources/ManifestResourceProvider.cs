using System.IO;
using System.Reflection;

namespace CSharpMath.Resources {
  public class ManifestResourceProvider {
    private readonly Assembly _resourceAssembly;
    /// <summary>Pass in the assembly where the resources are stored.</summary>
    public ManifestResourceProvider(Assembly resourceAssembly) =>
      _resourceAssembly = resourceAssembly;
    public Stream? ManifestStream(string resourceName) =>
      _resourceAssembly.GetManifestResourceStream
        (_resourceAssembly.ManifestResourcePrefix() + resourceName);
  }
}
