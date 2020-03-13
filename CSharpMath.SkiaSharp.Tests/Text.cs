using F = Xunit.InlineDataAttribute;
namespace CSharpMath.SkiaSharp {
  public class Text {
    [
      Xunit.Theory,
      F("x", "x")
    ]
    public void Test(string file, string latex) =>
      Tests.Test(file, latex, Tests.TextFolder, new TextPainter());
  }
}
