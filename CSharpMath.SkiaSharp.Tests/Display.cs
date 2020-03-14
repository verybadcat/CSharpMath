using Xunit;
using F = Xunit.InlineDataAttribute;
namespace CSharpMath.SkiaSharp {
  [Collection(nameof(Tests))]
  public class Display {
    private void Test(string file, string latex) =>
      Tests.Test(file, latex, Tests.DisplayFolder,
        new MathPainter { LineStyle = Atoms.LineStyle.Display });
    [Theory, ClassData(typeof(SharedData))]
    public void SharedTests(string file, string latex) => Test(file, latex);
    [Theory, ClassData(typeof(MathData))]
    public void MathTests(string file, string latex) => Test(file, latex);
  }
}
