using CSharpMath.Rendering;
using SkiaSharp;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.SkiaSharp {
  public class TextPainter : TextPainter<SKCanvas, SKColor> {
    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;
    public override Color WrapColor(SKColor color) => color.FromNative();
    public override SKColor UnwrapColor(Color color) => color.ToNative();
    public override ICanvas WrapCanvas(SKCanvas canvas) =>
      new SkiaCanvas(canvas, SKStrokeCap.Butt, MathPainter.DefaultAntiAlias);
  }
}
