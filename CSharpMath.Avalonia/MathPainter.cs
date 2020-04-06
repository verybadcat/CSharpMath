using AvaloniaColor = Avalonia.Media.Color;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = CSharpMath.Structures.Color;

namespace CSharpMath.Avalonia {
  public sealed class MathPainter : MathPainter<AvaloniaCanvas, AvaloniaColor> {
    public override AvaloniaColor UnwrapColor(CSharpMathColor color) => color.ToAvaloniaColor();

    public override ICanvas WrapCanvas(AvaloniaCanvas canvas) => canvas;

    public override CSharpMathColor WrapColor(AvaloniaColor color) => color.ToCSharpMathColor();
  }
}
