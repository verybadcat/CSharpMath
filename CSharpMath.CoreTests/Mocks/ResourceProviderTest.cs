using CSharpMath.Resources;
using Xunit;

namespace CSharpMath.CoreTests.Mocks {
  public class ResourceProviderTest {
    [Fact]
    public void ResourceProvider_FindsResource() =>
      Assert.NotNull(ManifestResources.LatinMathContent);
    [Fact]
    public void ResourceProvider_FindsMathConfiguration() =>
      Assert.IsType<Newtonsoft.Json.Linq.JObject>(ManifestResources.LatinMath["constants"]);
  }
}
