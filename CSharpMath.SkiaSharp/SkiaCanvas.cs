using CSharpMath.Rendering;
using CSharpMath.Structures;
using SkiaSharp;
namespace CSharpMath.SkiaSharp
{
  public class SkiaCanvas : ICanvas {
    public SkiaCanvas(SKCanvas canvas, SKStrokeCap strokeCap, bool? antiAliasLevel) {
      Canvas = canvas;
      StrokeCap = strokeCap;
      _paint = new SKPaint {
        IsAntialias = antiAliasLevel != null,
        SubpixelText = antiAliasLevel == true,
        HintingLevel = SKPaintHinting.Full
      };
      _path = null;
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

    private SKPaint _paint;
    private SKPath _path;

    private SKPaint StyledPaint(PaintStyle style, float? strokeWidth = null) {
      //In SkiaSharp, line will still be visible even if width is 0
      _paint.Color = strokeWidth == 0 ? SKColors.Transparent : (CurrentColor ?? DefaultColor).ToNative();
      _paint.Style = (SKPaintStyle)style;
      _paint.StrokeWidth = strokeWidth ?? 0;
      _paint.StrokeCap = StrokeCap;
      return _paint;
    }
    private SKPaint Paint => StyledPaint(CurrentStyle);
    private SKColor Color => CurrentColor.ToNative() ?? DefaultColor.ToNative();

    //Path methods
    public void BeginRead(int contourCount) => _path = new SKPath();
    public void EndRead() { Canvas.DrawPath(_path, Paint); _path.Dispose(); _path = null; }
    public void CloseContour() => _path.Close();
    public void Curve3(float x1, float y1, float x2, float y2) => _path.QuadTo(x1, y1, x2, y2);
    public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => _path.CubicTo(x1, y1, x2, y2, x3, y3);
    public void LineTo(float x1, float y1) => _path.LineTo(x1, y1);
    public void MoveTo(float x0, float y0) { _path.Close(); _path.MoveTo(x0, y0); }

    //Canvas methods
    public void StrokeRect(float left, float top, float width, float height) => Canvas.DrawRect(SKRect.Create(left, top, width, height), StyledPaint(PaintStyle.Stroke));
    public void FillRect(float left, float top, float width, float height) => Canvas.DrawRect(SKRect.Create(left, top, width, height), StyledPaint(PaintStyle.Fill));
    public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness) {
      if (CurrentStyle == PaintStyle.Fill)
        Canvas.DrawLine(x1, y1, x2, y2, StyledPaint(PaintStyle.Stroke, lineThickness));
      else this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
    }
    public void Save() => Canvas.Save();
    public void Translate(float dx, float dy) => Canvas.Translate(dx, dy);
    public void Scale(float sx, float sy) => Canvas.Scale(sx, sy);
    public void Restore() => Canvas.Restore();
    public void FillText(string text, float x, float y, float pointSize) => Canvas.DrawText(text, x, y, new SKPaint { Color = Color, TextSize = pointSize, IsAntialias = true });
  }
}
