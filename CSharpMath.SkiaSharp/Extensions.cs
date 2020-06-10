using CSharpMath.Structures;
using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public static class Extensions {
    public static SKColor ToNative(this Color color) =>
      new SKColor(color.R, color.G, color.B, color.A);
    public static Color FromNative(this SKColor color) =>
      new Color(color.Red, color.Green, color.Blue, color.Alpha);
    public static System.IO.Stream? DrawAsStream<TContent>
      (this Painter<SKCanvas, TContent, SKColor> painter,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       TextAlignment alignment = TextAlignment.TopLeft,
       SKEncodedImageFormat format = SKEncodedImageFormat.Png,
       int quality = 100) where TContent : class {
      if (!(painter.Measure(textPainterCanvasWidth) is { } size)) return null;
      using var surface = SKSurface.Create(new SKImageInfo((int)size.Width, (int)size.Height));
      painter.Draw(surface.Canvas, alignment);
      using var snapshot = surface.Snapshot();
      return snapshot.Encode(format, quality).AsStream();
    }
  }
}