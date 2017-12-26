using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Tests.Resources {
  public static class TestResources {
    private static JToken _latinMath;
    public static JToken LatinMath {
      get {
        if (_latinMath == null) {
          var provider = TestResourceProviders.Provider;
          return provider.M
        }
      }
    }
  }
}
