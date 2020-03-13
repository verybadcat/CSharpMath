using F = Xunit.InlineDataAttribute;
namespace CSharpMath.SkiaSharp {
  public class Inline {
    [
      Xunit.Theory,
      F("x", "x")
    ]
    public void Test(string file, string latex) =>
      Tests.Test(file, latex, Tests.InlineFolder,
        new MathPainter { LineStyle = Atoms.LineStyle.Text });
  }
}
