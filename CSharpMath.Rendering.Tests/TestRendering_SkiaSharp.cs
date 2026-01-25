using System.Linq;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using FrontEnd;
  using SkiaSharp;
  public class TestRendering_SkiaSharp : TestRendering<SKCanvas, SKColor, MathPainter, TextPainter> {
    protected override string FrontEnd => nameof(SkiaSharp);
    protected override double FileSizeTolerance => 0; // SkiaSharp is the baseline, no deviations allowed
    protected override void DrawToStream<TContent>(Painter<SKCanvas, TContent, SKColor> painter,
      System.IO.Stream stream, float textPainterCanvasWidth, TextAlignment alignment) =>
      painter.DrawAsStream(textPainterCanvasWidth, alignmentForTests: alignment)?.CopyTo(stream);
    public static TheoryData<string, MathPainter> MathPainterSettingsDataExtra =>
      new TheoryData<string, MathPainter> {
        { "NoAntiAlias", new MathPainter { AntiAlias = false } }
      };
    [Theory]
    [MemberData(nameof(MathPainterSettingsDataExtra))]
    public override void MathPainterSettings(string file, MathPainter painter) =>
      base.MathPainterSettings(file, painter);
  }
}
