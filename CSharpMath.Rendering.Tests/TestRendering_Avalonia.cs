using AvaloniaColor = Avalonia.Media.Color;

namespace CSharpMath.Rendering.Tests {
  using System.IO;
  using Avalonia;
  using FrontEnd;

  public class TestRendering_Avalonia : TestRendering<AvaloniaCanvas, AvaloniaColor, MathPainter, TextPainter> {
    protected override string FrontEnd => nameof(Avalonia);
    protected override double FileSizeTolerance => 0.0472;
    protected override void DrawToStream<TContent>(Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter,
      Stream stream, float textPainterCanvasWidth, TextAlignment alignment) =>
      painter.DrawAsPng(stream, textPainterCanvasWidth, null, alignment);
  }
}