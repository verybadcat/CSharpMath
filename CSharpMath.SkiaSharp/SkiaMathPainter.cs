using System.Drawing;
using CSharpMath.Rendering;

using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathPainter : MathPainter<SKCanvas>, IPainter<SKCanvas, MathSource, SKColor> {
    public SkiaMathPainter(float fontSize = 20f, bool antiAlias = true) : base(fontSize) =>
      AntiAlias = antiAlias;
    
    public SKStrokeCap StrokeCap { get; set; }
    public bool AntiAlias { get; set; }

    protected override ICanvas CreateCanvasWrapper(SKCanvas canvas) =>
      new SkiaCanvas(canvas, StrokeCap, AntiAlias);

    public new SKColor TextColor {
      get => base.TextColor.ToNative();
      set => base.TextColor = value.FromNative();
    }

    public new SKColor BackgroundColor {
      get => base.BackgroundColor.ToNative();
      set => base.BackgroundColor = value.FromNative();
    }
    public new SKColor ErrorColor {
      get => base.ErrorColor.ToNative();
      set => base.ErrorColor = value.FromNative();
    }
  }
}