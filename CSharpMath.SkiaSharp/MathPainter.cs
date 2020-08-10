using System.Drawing;
using CSharpMath.Structures;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;

namespace CSharpMath.SkiaSharp {
  public class MathPainter : MathPainter<SKCanvas, SKColor> {
    public bool AntiAlias { get; set; } = true;
    public void Draw(SKCanvas canvas, SKPoint point) => Draw(canvas, point.X, point.Y);
    public override SKColor UnwrapColor(Color color) => color.ToNative();
    public override Color WrapColor(SKColor color) => color.FromNative();
    public override ICanvas WrapCanvas(SKCanvas canvas) =>
      new SkiaCanvas(canvas, AntiAlias);
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// Repositions the <paramref name="display"/>.
    /// </summary>
    public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph>? display,
      SKCanvas canvas, PointF position) =>
        DrawDisplay(settings, display, _ => _.Draw(canvas, position));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// Repositions the <paramref name="display"/>.
    /// </summary>
    public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph>? display,
      SKCanvas canvas, SKPoint position) =>
        DrawDisplay(settings, display, _ => _.Draw(canvas, position));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// Repositions the <paramref name="display"/>.
    /// </summary>
    public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph>? display,
      SKCanvas canvas, float x, float y) =>
        DrawDisplay(settings, display, _ => _.Draw(canvas, x, y));
    /// <summary>
    /// Ignores the MathList and LaTeX of the <see cref="MathPainter"/> provided.
    /// Repositions the <paramref name="display"/>.
    /// </summary>
    public static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph>? display,
      SKCanvas canvas, TextAlignment textAlignment = TextAlignment.Center,
      Thickness padding = default, float offsetX = 0, float offsetY = 0) =>
          DrawDisplay(settings, display, _ =>
          _.Draw(canvas, textAlignment, padding, offsetX, offsetY));
    
    private static void DrawDisplay(MathPainter settings, Display.IDisplay<Fonts, Glyph>? display,
      System.Action<MathPainter> draw) {
      if (display is null) return;
      var original = (settings.Display, settings._displayChanged);
      (settings.Display, settings._displayChanged) = (display, false);
      draw(settings);
      (settings.Display, settings._displayChanged) = original;
    }
  }
}
