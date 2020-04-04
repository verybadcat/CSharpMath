using System.Linq;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  public class TestIcon {
    [Fact]
    public void DrawsAsUsual() {
      using var surface = SKSurface.Create(new SKImageInfo(258, 258));
      DrawIcon.Draw(surface.Canvas);
      using var expected = TestFixture.ThisDirectory.Parent.EnumerateFiles("Icon.png").Single().OpenRead();
      using var actual = surface.Snapshot().Encode().AsStream();
      Assert.Equal(expected.Length, actual.Length);
      Assert.True(TestFixture.StreamsContentsAreEqual(expected, actual));
    }
  }
}
