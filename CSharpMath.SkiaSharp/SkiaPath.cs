using SKPath = SkiaSharp.SKPath;
using System.Drawing;

namespace CSharpMath.SkiaSharp {
  public sealed class SkiaPath : Rendering.FrontEnd.Path {
    public SkiaPath(SkiaCanvas owner) => _owner = owner;
    public override Color? Foreground { get; set; }
    private readonly SkiaCanvas _owner;
    private readonly SKPath _path = new SKPath();
    public override void MoveTo(float x0, float y0) { _path.Close(); _path.MoveTo(x0, y0); }
    public override void LineTo(float x1, float y1) => _path.LineTo(x1, y1);
    public override void Curve3(float x1, float y1, float x2, float y2) =>
      _path.QuadTo(x1, y1, x2, y2);
    public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) =>
      _path.CubicTo(x1, y1, x2, y2, x3, y3);
    public override void CloseContour() => _path.Close();
    public override void Dispose() {
      if (Foreground is Color foreground)
        using (var paint = _owner.Paint.Clone()) {
          paint.Color = foreground.ToNative();
          _owner.Canvas.DrawPath(_path, paint);
        }
      else
        _owner.Canvas.DrawPath(_path, _owner.Paint);
      _path.Dispose();
    }
  }
}
