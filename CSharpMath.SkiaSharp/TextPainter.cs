using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;

namespace CSharpMath.SkiaSharp
{
  public class TextPainter : TextPainter<SKCanvas, SKColor> {
    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;

    public override SKColor UnwrapColor(Color color) => new SKColor(color.R, color.G, color.B, color.A);

    public override ICanvas WrapCanvas(SKCanvas canvas) => new SkiaCanvas(canvas, SKStrokeCap.Butt, true);

    public override Color WrapColor(SKColor color) => new Color(color.Red, color.Green, color.Blue, color.Alpha);
  }
}
