using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Resources;

namespace CSharpMath.Tests.Resources {
  public static class TestResourceProviders {
    public static ManifestResourceProvider Provider {
      get {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return new ManifestResourceProvider(assembly);
      }
    }
  }
}
