using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;
using System.Drawing;

namespace CSharpMath.SkiaSharp {
  public static class Extensions {
    public static SKColor ToNative(this Color color) =>
      new SKColor(color.R, color.G, color.B, color.A);
    public static Color FromNative(this SKColor color) =>
      Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    public static System.IO.Stream? DrawAsStream<TContent>
      (this Painter<SKCanvas, TContent, SKColor> painter,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       TextAlignment alignment = TextAlignment.TopLeft,
       SKEncodedImageFormat format = SKEncodedImageFormat.Png,
       int quality = 100) where TContent : class {
      var size = painter.Measure(textPainterCanvasWidth).Size;
      // SKSurface does not support zero width/height. Null will be returned from SKSurface.Create.
      if (size.Width is 0) size.Width = 1;
      if (size.Height is 0) size.Height = 1;
      using var surface = SKSurface.Create(new SKImageInfo((int)size.Width, (int)size.Height));
      painter.Draw(surface.Canvas, alignment);
      using var snapshot = surface.Snapshot();
      return snapshot.Encode(format, quality).AsStream();
    }
  }
}