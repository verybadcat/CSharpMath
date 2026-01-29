using Microsoft.Maui.Graphics;
using CSharpMathColor = System.Drawing.Color;
using CSharpMathICanvas = CSharpMath.Rendering.FrontEnd.ICanvas;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace CSharpMath.Maui {
  public sealed class TextPainter : CSharpMath.Rendering.FrontEnd.TextPainter<(ICanvas, SizeF), MauiColor> {
    public override MauiColor UnwrapColor(CSharpMathColor color) => color.ToMauiColor();

    public override CSharpMathICanvas WrapCanvas((ICanvas, SizeF) canvas) => new MauiCanvas(canvas);

    public override CSharpMathColor WrapColor(MauiColor color) => color.ToCSharpMathColor();
  }
}