using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CSharpMath.Resources {
  public static class ResourceProvider {
    public static byte[] ManifestContents(string resourceName) {
      Assembly a = Assembly.GetExecutingAssembly();
      var prefix = a.ManifestResourcePrefix();
      var path = prefix + resourceName;
      var stream = a.GetManifestResourceStream(path);
      if (stream == null) {
        return null;
      }
      byte[] r = _Buffer(stream);
      stream?.Dispose();
      return r;
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
