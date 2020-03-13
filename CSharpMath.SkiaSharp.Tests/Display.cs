using F = Xunit.InlineDataAttribute;
namespace CSharpMath.SkiaSharp {
  public class Display {
    [
      Xunit.Theory,
      F("Integral", @"\int^5_2x\ dx")
    ]
    public void Test(string file, string latex) =>
      Tests.Test(file, latex, Tests.DisplayFolder,
        new MathPainter { LineStyle = Atoms.LineStyle.Display });
  }
}
