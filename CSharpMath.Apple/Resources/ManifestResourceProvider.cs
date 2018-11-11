using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CSharpMath.Resources {
  public class ManifestResourceProvider {
    private readonly Assembly _resourceAssembly;

    /// <summary>Pass in the assembly where the resources are stored.</summary>
    public ManifestResourceProvider(Assembly resourceAssembly) {
      _resourceAssembly = resourceAssembly;
    }
    private Stream ManifestStream(string resourceName) {
      Assembly a = _resourceAssembly;
      var prefix = a.ManifestResourcePrefix();
      var path = prefix + resourceName;
      return a.GetManifestResourceStream(path);
    }
    public byte[] ManifestContents(string resourceName) {
      var stream = ManifestStream(resourceName);
      if (stream == null) {
        return null;
      }
      byte[] r = _Buffer(stream);
      stream?.Dispose();
      return r;
    }

    public string ManifestString(string resourceName) {
      var stream = ManifestStream(resourceName);
      var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    }

    private static byte[] _Buffer(Stream input) {
      byte[] r = null;
      if (input != null) {
        long length = input.Length;
        int intLength = (int)length;
        r = new byte[length];
        input.Read(r, 0, intLength);
      }
      return r;
    }
  }
}
