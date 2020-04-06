using System.Linq;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using FrontEnd;
  using SkiaSharp;
  public class TestSkiaSharp : Test<SKCanvas, SKColor, MathPainter, TextPainter> {
    protected override string FrontEnd => nameof(SkiaSharp);
    protected override double FileSizeTolerance => 0; // SkiaSharp is the baseline, no deviations allowed
    protected override void DrawToStream<TContent>(Painter<SKCanvas, TContent, SKColor> painter, System.IO.Stream stream, float textPainterCanvasWidth) =>
      painter.DrawAsStream(textPainterCanvasWidth)?.CopyTo(stream);
    [Fact]
    public override void MathPainterSettings() {
      base.MathPainterSettings();
      MathPainterSettingsTest("SquareStrokeCap", new MathPainter { StrokeCap = SKStrokeCap.Square });
      MathPainterSettingsTest("RoundStrokeCap", new MathPainter { StrokeCap = SKStrokeCap.Round });
      MathPainterSettingsTest("NoAntiAlias", new MathPainter { AntiAlias = false });
    }
  }
}
