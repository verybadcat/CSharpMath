using System.Drawing;
using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class MathPainter : MathPainter<SKCanvas, SKColor> {
    public bool AntiAlias { get; set; } = true;
    public void Draw(SKCanvas canvas, SKPoint point) => Draw(canvas, point.X, point.Y);
    public override SKColor UnwrapColor(Color color) => color.ToNative();
    public override Color WrapColor(SKColor color) => color.FromNative();
    public override ICanvas WrapCanvas(SKCanvas canvas) => new SkiaCanvas(canvas, AntiAlias);
  }
}
