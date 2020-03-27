using AvaloniaColor = Avalonia.Media.Color;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = CSharpMath.Structures.Color;

namespace CSharpMath.Avalonia {
  public sealed class MathPainter : MathPainter<AvaloniaCanvas, AvaloniaColor> {
    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;

    public override AvaloniaColor UnwrapColor(CSharpMathColor color) => color.ToAvaloniaColor();

    public override ICanvas WrapCanvas(AvaloniaCanvas canvas) => canvas;

    public override CSharpMathColor WrapColor(AvaloniaColor color) => color.ToCSharpMathColor();
  }
}
