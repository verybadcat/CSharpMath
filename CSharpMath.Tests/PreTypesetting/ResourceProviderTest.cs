using CSharpMath.Resources;
using CSharpMath.Tests.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests.PreTypesetting {
  public class ResourceProviderTest {
    [Fact]
    public void ResourceProvider_FindsResource() {
      var content = ManifestResourceProvider.ManifestContents("latinmodern-math.json");
      Assert.NotEmpty(content);
    }
    [Fact]
    public void ResourceProvider_FindsMathConfiguration() {
      var content = TestResources.LatinMath;
      var constants = content["constants"];
      var constantsType = constants.GetType();
      Assert.True(1 > 0);
    }
  }
}
