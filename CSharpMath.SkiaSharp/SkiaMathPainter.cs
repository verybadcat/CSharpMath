using System.Drawing;
using CSharpMath.Rendering;

using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathPainter : MathPainter, IPainter<MathSource, SKColor> {
    public SkiaMathPainter(SKSize bounds, float fontSize = 20f)
      : base(new SizeF(bounds.Width, bounds.Height), fontSize) { }
    public SkiaMathPainter(float width, float height, float fontSize = 20f)
      : base(width, height, fontSize) { }
    
    public SKStrokeCap StrokeCap { get; set; }
    public bool AntiAlias { get; set; } = true;

    new void Draw(ICanvas _) => throw null;
    public void Draw(SKCanvas canvas) => base.Draw(new SkiaCanvas(canvas, StrokeCap, AntiAlias));

    public new SKSize Bounds {
      get => new SKSize(base.Bounds.Width, base.Bounds.Height);
      set => base.Bounds = new SizeF(value.Width, value.Height);
    }

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