using SizeF = System.Drawing.SizeF;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;

using CSharpMath.Rendering.FrontEnd;
using CSharpMathTextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;

namespace CSharpMath.Avalonia {
  public static class Extensions {
    public static AvaloniaColor ToAvaloniaColor(this System.Drawing.Color color) =>
        new AvaloniaColor(color.A, color.R, color.G, color.B);

    internal static System.Drawing.Color ToCSharpMathColor(this AvaloniaColor color) =>
        System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    internal static CSharpMathTextAlignment ToCSharpMathTextAlignment(this AvaloniaTextAlignment alignment) =>
      alignment switch
      {
        AvaloniaTextAlignment.Left => CSharpMathTextAlignment.TopLeft,
        AvaloniaTextAlignment.Center => CSharpMathTextAlignment.Top,
        AvaloniaTextAlignment.Right => CSharpMathTextAlignment.TopRight,
        _ => CSharpMathTextAlignment.Left
      };

    public static SolidColorBrush ToSolidColorBrush(this System.Drawing.Color color) =>
        new SolidColorBrush(color.ToAvaloniaColor());

    class DrawVisual<TContent> : Visual where TContent : class {
      readonly Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter;
      readonly System.Drawing.RectangleF measure;
      readonly CSharpMathTextAlignment alignment;
      public DrawVisual(Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter,
        System.Drawing.RectangleF measure, CSharpMathTextAlignment alignment) {
        this.painter = painter;
        this.measure = measure;
        this.alignment = alignment;
      }
      public override void Render(DrawingContext context) {
        base.Render(context);
        var canvas = new AvaloniaCanvas(context, new Size(measure.Width, measure.Height));
        painter.Draw(canvas, alignment);
      }
    }
    public static void DrawAsPng<TContent>
      (this Painter<AvaloniaCanvas, TContent, AvaloniaColor> painter,
       System.IO.Stream target,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       CSharpMathTextAlignment alignment = CSharpMathTextAlignment.TopLeft) where TContent : class {
      if (!(painter.Measure(textPainterCanvasWidth) is { } size)) return;
      using var bitmap =
        new RenderTargetBitmap(new PixelSize((int)size.Width, (int)size.Height));
      bitmap.Render(new DrawVisual<TContent>(painter, size, alignment));
      bitmap.Save(target);
    }
  }
}
