using AvaloniaColor = Avalonia.Media.Color;

namespace CSharpMath.Rendering.Tests {
  using System.IO;
  using Avalonia;
  using FrontEnd;
  public class TestAvalonia : Test<AvaloniaCanvas, AvaloniaColor, MathPainter, TextPainter> {
    protected override string FrontEnd => nameof(Avalonia);
    protected override double FileSizeTolerance => 0.00466580472;
    protected override void DrawToStream<TContent>(Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter, Stream stream, float textPainterCanvasWidth) =>
      painter.DrawAsPng(stream, textPainterCanvasWidth);
  }
}
