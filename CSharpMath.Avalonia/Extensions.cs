using SizeF = System.Drawing.SizeF;

using Avalonia;
using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = CSharpMath.Structures.Color;
using CSharpMathTextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;

namespace CSharpMath.Avalonia {
  public static class Extensions {
    public static AvaloniaColor ToAvaloniaColor(this CSharpMathColor color) =>
        new AvaloniaColor(color.A, color.R, color.G, color.B);

    public static Size ToAvaloniaSize(this SizeF size) =>
        new Size(size.Width, size.Height);

    internal static CSharpMathColor ToCSharpMathColor(this AvaloniaColor color) =>
        new CSharpMathColor(color.R, color.G, color.B, color.A);

    internal static CSharpMathTextAlignment ToCSharpMathTextAlignment(this AvaloniaTextAlignment alignment) =>
      alignment switch
      {
        AvaloniaTextAlignment.Left => CSharpMathTextAlignment.TopLeft,
        AvaloniaTextAlignment.Center => CSharpMathTextAlignment.Top,
        AvaloniaTextAlignment.Right => CSharpMathTextAlignment.TopRight,
        _ => CSharpMathTextAlignment.Left
      };

    public static SolidColorBrush ToSolidColorBrush(this CSharpMathColor color) =>
        new SolidColorBrush(color.ToAvaloniaColor());

    class DrawVisual<TContent> : Visual where TContent : class {
      readonly Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter;
      readonly System.Drawing.RectangleF measure;
      public DrawVisual(Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter,
        System.Drawing.RectangleF measure) {
        this.painter = painter;
        this.measure = measure;
      }
      public override void Render(DrawingContext context) {
        base.Render(context);
        var canvas = new AvaloniaCanvas(context, new Size(measure.Width, measure.Height));
        painter.Draw(canvas, CSharpMathTextAlignment.TopLeft);
      }
    }
    public static void DrawAsPng<TContent>
      (this Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter,
       System.IO.Stream target,
       float textPainterCanvasWidth = 2000f) where TContent : class {
      if (!(painter.Measure(textPainterCanvasWidth) is { } size)) return;
      using var bitmap =
        new global::Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize((int)size.Width, (int)size.Height));
      bitmap.Render(new DrawVisual<TContent>(painter, size));
      bitmap.Save(target);
    }
  }
}
