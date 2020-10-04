using CSharpMath.CoreTests.FrontEnd;
using Xunit;
using TGlyph = System.Char;
using CSharpMath.Display;

namespace CSharpMath.CoreTests {
  // purpose of this class is to make sure our mocks behave as expected.
  public class MockTests {
    [Fact]
    public void TestGlyphBoundsWithoutM() {
      string hello = "Hello";
      var font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(hello, hello, font);
      Assert.All(glyphRun.GlyphInfos, glyphInfo => Assert.Null(glyphInfo.Foreground));
      var width = provider.GetTypographicWidth(font, glyphRun);
      Approximately.Equal(width, 25,  0.01);
    }
    [Fact]
    public void TestGlyphBoundsWithM() {
      string america = "America";
      var font = new TestFont(10);
      var provider = TestGlyphBoundsProvider.Instance;
      var glyphRun = new AttributedGlyphRun<TestFont, TGlyph>(america, america, font);
      Assert.All(glyphRun.GlyphInfos, glyphInfo => Assert.Null(glyphInfo.Foreground));
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
