using System.Drawing;
using System.Linq;
using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public static class Extensions {
    public static SKColor ToNative(this Color color) =>
      new SKColor(color.R, color.G, color.B, color.A);
    public static Color FromNative(this SKColor color) =>
      Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    public static System.IO.Stream? DrawAsStream<TContent>
      (this Painter<SKCanvas, TContent, SKColor> painter,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       SKEncodedImageFormat format = SKEncodedImageFormat.Png,
       int quality = 100,
       TextAlignment alignmentForTests = TextAlignment.TopLeft) where TContent : class {
      var measure = painter.Measure(textPainterCanvasWidth);
      var display = painter.Display;
      var expandsLegacyBounds = painter is CSharpMath.Rendering.FrontEnd.TextPainter<SKCanvas, SKColor> text
        ? text._absoluteXCoordDisplay.Displays.Any(item =>
          DisplayInkBounds.ContainsMultipleRows(item) && DisplayInkBounds.ExtendsOwnAdvance(item))
        : display != null && DisplayInkBounds.ContainsMultipleRows(display)
          && DisplayInkBounds.ExtendsOwnAdvance(display);
      if (!expandsLegacyBounds) {
        var legacyWidth = System.Math.Max(1, (int)(display?.Width ?? measure.Width));
        var legacyHeight = System.Math.Max(1, (int)(display is null
          ? measure.Height : display.Ascent + display.Descent));
        using var legacySurface = SKSurface.Create(new SKImageInfo(legacyWidth, legacyHeight));
        painter.Draw(legacySurface.Canvas, alignmentForTests);
        using var legacySnapshot = legacySurface.Snapshot();
        return legacySnapshot.Encode(format, quality).AsStream();
      }

      // Bounds-aware output is isolated to displays whose descendants extend
      // outside the legacy root advance. Keep a one-pixel guard around that
      // ink so antialiasing at an exact glyph edge is not clipped.
      var width = System.Math.Max(1, (int)System.Math.Ceiling(measure.Width) + 8);
      var height = System.Math.Max(1, (int)System.Math.Ceiling(measure.Height) + 4);
      var origin = display is null ? default : IPainterExtensions.GetDisplayPosition(
        display.Width, display.Ascent, display.Descent, painter.FontSize,
        width, height, alignmentForTests, default, 0, 0);
      // Measure uses mathematical coordinates while the canvas is vertically
      // inverted. Translate both extrema into the allocated surface.
      var offsetX = 4 - measure.Left - origin.X;
      var offsetY = origin.Y - measure.Top + 1;
      using var surface = SKSurface.Create(new SKImageInfo(width, height));
      if (painter is CSharpMath.Rendering.FrontEnd.TextPainter<SKCanvas, SKColor> textPainter)
        textPainter.DrawAtLayoutWidth(surface.Canvas, textPainterCanvasWidth,
          alignmentForTests, offsetX, offsetY);
      else
        painter.Draw(surface.Canvas, alignmentForTests, offsetX: offsetX, offsetY: offsetY);
      using var snapshot = surface.Snapshot();
      return snapshot.Encode(format, quality).AsStream();
    }
  }
}
