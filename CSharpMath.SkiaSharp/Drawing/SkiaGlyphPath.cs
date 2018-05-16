namespace CSharpMath.SkiaSharp
{
  public class SkiaGlyphPath : Typography.OpenFont.IGlyphTranslator {
    public global::SkiaSharp.SKPath Path { get; private set; } = new global::SkiaSharp.SKPath();

    public void BeginRead(int contourCount) { }
    public void EndRead() { }
    public void Clear() => Path.Reset();
    public void CloseContour() => Path.Close();
    public void Curve3(float x1, float y1, float x2, float y2) => Path.QuadTo(x1, y1, x2, y2);
    public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => Path.CubicTo(x1, y1, x2, y2, x3, y3);
    public void LineTo(float x1, float y1) => Path.LineTo(x1, y1);
    public void MoveTo(float x0, float y0) => Path.MoveTo(x0, y0);
  }
}
