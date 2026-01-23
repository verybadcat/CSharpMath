using System.Drawing;
using CSharpMath.Rendering.FrontEnd;
using VectSharp;

namespace CSharpMath.VectSharp;

public class MathPainter : MathPainter<Page, Colour> {
  public void Draw(Page canvas, global::VectSharp.Point point) => Draw(canvas, (float)point.X, (float)point.Y);
  public override Colour UnwrapColor(Color color) => color.ToNative();
  public override Color WrapColor(Colour color) => color.FromNative();
  public override ICanvas WrapCanvas(Page canvas) => new VectSharpCanvas(canvas);
}