using CSharpMath.CoreTests.FrontEnd;
using Xunit;
using TGlyph = System.Char;
using CSharpMath.Display;
using System.Linq;

namespace CSharpMath.CoreTests {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      var font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(hello, hello, font);
      Assert.True(glyphRun.GlyphInfos.All(GlyphInfo => GlyphInfo.Foreground == null));
      var width = provider.GetTypographicWidth(font, glyphRun);
      Approximately.Equal(width, 25,  0.01);
    }

    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      var font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(america, america, font);
      Assert.True(glyphRun.GlyphInfos.All(GlyphInfo => GlyphInfo.Foreground == null));
      var width = provider.GetTypographicWidth(font, glyphRun);
      Approximately.Equal(width, 40, 0.01);
    }
    [Fact]
    public void ResourceProviderFindsResource() =>
      Assert.NotNull(Resources.ManifestResources.LatinMathContent);
    [Fact]
    public void ResourceProviderFindsMathConfiguration() =>
      Assert.IsType<Newtonsoft.Json.Linq.JObject>(Resources.ManifestResources.LatinMath["constants"]);
  }
}
