using CSharpMath.Rendering;
using CSharpMath.Rendering.Renderer;
using CSharpMath.Structures;
using SkiaSharp;
namespace CSharpMath.SkiaSharp {
  public enum AntiAlias { Disable, Enable, WithSubpixelText }
  public class SkiaCanvas : ICanvas {
    public SkiaCanvas(SKCanvas canvas, SKStrokeCap strokeCap, AntiAlias antiAliasLevel) {
      Canvas = canvas;
      StrokeCap = strokeCap;
      _paint = new SKPaint {
        IsAntialias = antiAliasLevel != AntiAlias.Disable,
        SubpixelText = antiAliasLevel == AntiAlias.WithSubpixelText,
        HintingLevel = SKPaintHinting.Full
      };
      CurrentStyle = default;
      CurrentColor = default;
      DefaultColor = default;
    }
    public SKCanvas Canvas { get; }
    public float Width => Canvas.LocalClipBounds.Width;
    public float Height => Canvas.LocalClipBounds.Height;
    public SKStrokeCap StrokeCap { get; set; }
    public Color DefaultColor { get; set; }
    public Color? CurrentColor { get; set; }
    public PaintStyle CurrentStyle { get; set; }

    private readonly SKPaint _paint;
    private SKPaint StyledPaint(PaintStyle style, float? strokeWidth = null) {
      //In SkiaSharp, line will still be visible even if width is 0
      _paint.Color =
        strokeWidth == 0
        ? SKColors.Transparent
        : (CurrentColor ?? DefaultColor).ToNative();
      _paint.Style = (SKPaintStyle)style;
      _paint.StrokeWidth = strokeWidth ?? 0;
      _paint.StrokeCap = StrokeCap;
      return _paint;
    }
    internal SKPaint Paint => StyledPaint(CurrentStyle);
    private SKColor Color => CurrentColor?.ToNative() ?? DefaultColor.ToNative();

    //Canvas methods
    public void StrokeRect(float left, float top, float width, float height) =>
      Canvas.DrawRect(
        SKRect.Create(left, top, width, height), StyledPaint(PaintStyle.Stroke));
    public void FillRect(float left, float top, float width, float height) =>
      Canvas.DrawRect(
        SKRect.Create(left, top, width, height), StyledPaint(PaintStyle.Fill));
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      if (CurrentStyle == PaintStyle.Fill)
        Canvas.DrawLine(x1, y1, x2, y2, StyledPaint(PaintStyle.Stroke, lineThickness));
      else this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
    }
    public void Save() => Canvas.Save();
    public void Translate(float dx, float dy) => Canvas.Translate(dx, dy);
    public void Scale(float sx, float sy) => Canvas.Scale(sx, sy);
    public void Restore() => Canvas.Restore();
    public void FillText(string text, float x, float y, float pointSize) =>
      Canvas.DrawText(text, x, y, new SKPaint {
        Color = Color, TextSize = pointSize, IsAntialias = true
      });
    public IPath GetPath() => new SkiaPath(this);
  }
}
