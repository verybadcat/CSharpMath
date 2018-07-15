using System.Drawing;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathPainter : MathPainter<SKCanvas, SKColor>, IPainter<SKCanvas, MathSource, SKColor> {
    public SkiaMathPainter(float fontSize = 20f, bool antiAlias = true) : base(fontSize) =>
      AntiAlias = antiAlias;
    
    public SKStrokeCap StrokeCap { get; set; }
    public bool AntiAlias { get; set; }

    public override SKColor UnwrapColor(Color color) => color.ToNative();

    public override Color WrapColor(SKColor color) => color.FromNative();

    public override ICanvas WrapCanvas(SKCanvas canvas) =>
      new SkiaCanvas(canvas, StrokeCap, AntiAlias);

  }
}