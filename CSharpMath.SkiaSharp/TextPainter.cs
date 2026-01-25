using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;
using System.Drawing;

namespace CSharpMath.SkiaSharp {
  public class TextPainter : TextPainter<SKCanvas, SKColor> {
    public bool AntiAlias { get; set; } = true;
    public override Color WrapColor(SKColor color) => color.FromNative();
    public override SKColor UnwrapColor(Color color) => color.ToNative();
    public override ICanvas WrapCanvas(SKCanvas canvas) => new SkiaCanvas(canvas, AntiAlias);
  }
}
