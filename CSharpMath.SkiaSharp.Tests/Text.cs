using Xunit;
using F = Xunit.InlineDataAttribute;
namespace CSharpMath.SkiaSharp {
  [Collection(nameof(Tests))]
  public class Text {
    void Test(string file, string latex) =>
      Tests.Test(file, latex, Tests.TextFolder, new TextPainter());
    [Theory, ClassData(typeof(SharedData))]
    public void SharedTests(string file, string latex) => Test(file, latex);
    [
      Theory,
      F("Accent", @"\'a")
    ]
    public void TextTests(string file, string latex) => Test(file, latex);
  }
}
