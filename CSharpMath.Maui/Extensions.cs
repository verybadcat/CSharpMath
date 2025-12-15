using Microsoft.Maui;
using Microsoft.Maui.Controls;

using CSharpMathColor = System.Drawing.Color;
using MauiColor = Microsoft.Maui.Graphics.Color;
using CSharpMathTextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;
using MauiICanvas = Microsoft.Maui.Graphics.ICanvas;

namespace CSharpMath.Maui {
  public static class Extensions {
    public static MauiColor ToMauiColor(this CSharpMathColor color) =>
        MauiColor.FromRgba(color.R, color.G, color.B, color.A);

    internal static CSharpMathColor ToCSharpMathColor(this MauiColor color) {
      color.ToRgba(out var r, out var g, out var b, out var a);
      return CSharpMathColor.FromArgb(a, r, g, b);
    }

    internal static CSharpMathTextAlignment ToCSharpMathTextAlignment(this TextAlignment alignment) =>
      alignment switch
      {
        TextAlignment.Start => CSharpMathTextAlignment.TopLeft,
        TextAlignment.Center => CSharpMathTextAlignment.Top,
        TextAlignment.End => CSharpMathTextAlignment.TopRight,
        _ => CSharpMathTextAlignment.Left
      };

    public static void DrawAsPng<TContent>
      (this Rendering.FrontEnd.Painter<MauiICanvas, TContent, MauiColor> painter,
       System.IO.Stream target,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       CSharpMathTextAlignment alignment = CSharpMathTextAlignment.TopLeft) where TContent : class {
      if (!(painter.Measure(textPainterCanvasWidth) is { } size)) return;
      // In case there is no support for zero width/height - other frontends have this check
      // and I won't waste time checking each Maui platform to validate this.
      if (size.Width is 0) size.Width = 1;
      if (size.Height is 0) size.Height = 1;
      using var context = new Microsoft.Maui.Graphics.Platform.PlatformBitmapExportService().CreateContext((int)size.Width, (int)size.Height);
      painter.Draw(context.Canvas, alignment);
      context.WriteToStream(target);
    }
  }
}
