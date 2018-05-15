namespace CSharpMath.SkiaSharp
{
  public class SkiaGlyphPath : DrawingGL.IWritablePath {
    public global::SkiaSharp.SKPath Path { get; } = new global::SkiaSharp.SKPath();
    public void BezireTo(float x1, float y1, float x2, float y2, float x3, float y3) => Path.CubicTo(x1, y1, x2, y2, x3, y3);
    public void CloseFigure() => Path.Close();
    public void LineTo(float x1, float y1) => Path.LineTo(x1, y1);
    public void MoveTo(float x0, float y0) => Path.MoveTo(x0, y0);
  }
}
