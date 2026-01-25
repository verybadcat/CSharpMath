using Microsoft.Maui.Graphics;
using CSharpMathICanvas = CSharpMath.Rendering.FrontEnd.ICanvas;
using MauiColor = Microsoft.Maui.Graphics.Color;
using CSharpMathColor = System.Drawing.Color;

namespace CSharpMath.Maui {
  public sealed class MathPainter : CSharpMath.Rendering.FrontEnd.MathPainter<(ICanvas, SizeF), MauiColor> {
    public override MauiColor UnwrapColor(CSharpMathColor color) => color.ToMauiColor();

    public override CSharpMathICanvas WrapCanvas((ICanvas, SizeF) canvas) => new MauiCanvas(canvas);

    public override CSharpMathColor WrapColor(MauiColor color) => color.ToCSharpMathColor();
  }
}
