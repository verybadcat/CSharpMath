using CSharpMath.Rendering.FrontEnd;
using AvaloniaColor = Avalonia.Media.Color;
using CSharpMathColor = System.Drawing.Color;

namespace CSharpMath.Avalonia {
  public sealed class TextPainter : TextPainter<AvaloniaCanvas, AvaloniaColor> {
    public override AvaloniaColor UnwrapColor(CSharpMathColor color) => color.ToAvaloniaColor();

    public override ICanvas WrapCanvas(AvaloniaCanvas canvas) => canvas;

    public override CSharpMathColor WrapColor(AvaloniaColor color) => color.ToCSharpMathColor();
  }
}