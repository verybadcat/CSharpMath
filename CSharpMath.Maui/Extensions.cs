using System;
using System.IO;
using CSharpMath.Rendering.FrontEnd;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CSharpMathColor = System.Drawing.Color;
using CSharpMathTextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiICanvas = Microsoft.Maui.Graphics.ICanvas;
using MauiTextAlignment = Microsoft.Maui.TextAlignment;

namespace CSharpMath.Maui {
  public static class Extensions {
    public static MauiColor ToMauiColor(this CSharpMathColor color) =>
        MauiColor.FromRgba(color.R, color.G, color.B, color.A);

    public static CSharpMathColor ToCSharpMathColor(this MauiColor color) {
      color.ToRgba(out var r, out var g, out var b, out var a);
      return CSharpMathColor.FromArgb(a, r, g, b);
    }

    internal static CSharpMathTextAlignment ToCSharpMathTextAlignment(this MauiTextAlignment alignment) =>
      alignment switch {
        MauiTextAlignment.Start => CSharpMathTextAlignment.TopLeft,
        MauiTextAlignment.Center => CSharpMathTextAlignment.Top,
        MauiTextAlignment.End => CSharpMathTextAlignment.TopRight,
        _ => CSharpMathTextAlignment.Left
      };
#if ANDROID || IOS || MACCATALYST || WINDOWS
    public static System.Threading.Tasks.Task DrawToStreamAsync<TContent>
      (this Rendering.FrontEnd.Painter<(MauiICanvas, SizeF), TContent, MauiColor> painter,
       System.IO.Stream target,
       float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth,
       ImageFormat format = ImageFormat.Png,
       float quality = 1,
       CSharpMathTextAlignment alignmentForTests = CSharpMathTextAlignment.TopLeft) where TContent : class {
      if (!(painter.Measure(textPainterCanvasWidth) is { } size)) return System.Threading.Tasks.Task.CompletedTask;
      // In case there is no support for zero width/height - other frontends have this check
      // and I won't waste time checking each Maui platform to validate this.
      if (size.Width is 0) size.Width = 1;
      if (size.Height is 0) size.Height = 1;
#if WINDOWS
      var device = Microsoft.Graphics.Canvas.CanvasDevice.GetSharedDevice();
      using var canvas = new Microsoft.Maui.Graphics.Platform.PlatformCanvas();
      using var renderTarget = new Microsoft.Graphics.Canvas.CanvasRenderTarget(device, size.Width, size.Height, 96);
      canvas.CanvasSize = renderTarget.Size;
      using (canvas.Session = renderTarget.CreateDrawingSession()) {
        painter.Draw((canvas, new SizeF(size.Width, size.Height)), alignmentForTests);
      }
      var bs = renderTarget.GetPixelBytes();
      var nativeFormat = format switch {
        ImageFormat.Jpeg => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Jpeg,
        ImageFormat.Png => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Png,
        ImageFormat.Gif => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Gif,
        ImageFormat.Tiff => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Tiff,
        ImageFormat.Bmp => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Bmp,
        _ => Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Png,
      };
      return renderTarget.SaveAsync(target.AsRandomAccessStream(), nativeFormat, quality).AsTask();
#else
      using var context = new Microsoft.Maui.Graphics.Platform.PlatformBitmapExportService().CreateContext((int)size.Width, (int)size.Height);
      painter.Draw((context.Canvas, new SizeF(size.Width, size.Height)), alignmentForTests);
      return context.Image.SaveAsync(target, format, quality);
#endif
    }
#endif
    class KeyboardDrawable(MathKeyboard keyboard, MathPainter settings, Color caretColor, CaretShape caretShape) : IDrawable {
      public void Draw(MauiICanvas canvas, RectF dirtyRect) {
        settings.DrawDisplay(keyboard.Display, (canvas, dirtyRect.Size));
        if (keyboard.ShouldDrawCaret)
          keyboard.DrawCaret(new MauiCanvas((canvas, dirtyRect.Size)), caretColor.ToCSharpMathColor(), caretShape);
      }
    }
    /// <summary>
    /// Registers event handling for a <see cref="GraphicsView"/> to render a <see cref="MathKeyboard"/> and move its caret on touch.
    /// Do not call this method more than once for the same <see cref="MathKeyboard"/> and <see cref="GraphicsView"/> or
    /// the same event handlers will be registered multiple times.
    /// </summary>
    public static void BindDisplay(this MathKeyboard keyboard,
      GraphicsView view, MathPainter settings, Color caretColor,
      CaretShape caretShape = CaretShape.IBeam) {
      view.StartInteraction +=
        (sender, e) => {
          keyboard.MoveCaretToPoint(new System.Drawing.PointF(e.Touches[0].X, e.Touches[0].Y));
        };
      keyboard.RedrawRequested += (_, _) => view.Invalidate();
      view.Drawable = new KeyboardDrawable(keyboard, settings, caretColor, caretShape);
    }
    /// <summary>
    /// Updates caret color and shape for a bound keyboard view.
    /// Only call this after calling <see cref="BindDisplay(MathKeyboard, GraphicsView, MathPainter, Color, CaretShape)"/>.
    /// </summary>
    public static void UpdateDisplayCaret(this MathKeyboard keyboard, GraphicsView view, MathPainter settings, Color caretColor,
      CaretShape caretShape = CaretShape.IBeam) {
      view.Drawable = new KeyboardDrawable(keyboard, settings, caretColor, caretShape);
    }
  }
}