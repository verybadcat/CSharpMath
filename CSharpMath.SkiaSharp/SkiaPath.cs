using SKPath = SkiaSharp.SKPath;

namespace CSharpMath.SkiaSharp {
  using Structures;
  public class SkiaPath : Rendering.IPath {
    public SkiaPath(SkiaCanvas owner) => _owner = owner;

    public Color? Foreground { private get; set; }
    private SkiaCanvas _owner;
    private SKPath _path = new SKPath();
    public void BeginRead(int contourCount) { }
    public void EndRead() {
      if (Foreground is Color foreground)
        using (var paint = _owner.Paint.Clone()) {
          paint.Color = foreground.ToNative();
          _owner.Canvas.DrawPath(_path, paint);
        }
      else
        _owner.Canvas.DrawPath(_path, _owner.Paint);
      _path.Dispose();
      _path = null;
      _owner = null;
    }
    public void CloseContour() => _path.Close();
    public void Curve3(float x1, float y1, float x2, float y2) => _path.QuadTo(x1, y1, x2, y2);
    public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => _path.CubicTo(x1, y1, x2, y2, x3, y3);
    public void LineTo(float x1, float y1) => _path.LineTo(x1, y1);
    public void MoveTo(float x0, float y0) { _path.Close(); _path.MoveTo(x0, y0); }
  }
}
