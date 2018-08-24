using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;

namespace CSharpMath.SkiaSharp
{
  public class TextPainter : TextPainter<SKCanvas, SKColor> {
    protected override bool CoordinatesFromBottomLeftInsteadOfTopLeft => false;

    public override Color WrapColor(SKColor color) => color.FromNative();

    public override SKColor UnwrapColor(Color color) => color.ToNative();

    public override ICanvas WrapCanvas(SKCanvas canvas) => new SkiaCanvas(canvas, SKStrokeCap.Butt, true);
  }
}
