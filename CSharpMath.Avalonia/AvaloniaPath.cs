using Avalonia;
using Avalonia.Media;
using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = System.Drawing.Color;

namespace CSharpMath.Avalonia {
  public sealed class AvaloniaPath : Path {
    private readonly AvaloniaCanvas _canvas;
    private readonly PathGeometry _path = new PathGeometry();
    private readonly StreamGeometryContext _context;
    private bool _isOpen = true;
    public AvaloniaPath(AvaloniaCanvas canvas) { _canvas = canvas; _context = _path.Open(); }
    public override CSharpMathColor? Foreground { get; set; }
    public override void MoveTo(float x0, float y0) {
      if (_isOpen) { _context.EndFigure(false); _isOpen = false; }
      // The second parameter does nothing on Skia: https://github.com/AvaloniaUI/Avalonia/issues/1419
      _context.BeginFigure(new Point(x0, y0), _canvas.CurrentStyle == PaintStyle.Fill);
    }
    public override void LineTo(float x1, float y1) =>
      _context.LineTo(new Point(x1, y1));
    public override void Curve3(float x1, float y1, float x2, float y2) =>
        _context.QuadraticBezierTo(new Point(x1, y1), new Point(x2, y2));
    public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) =>
        _context.CubicBezierTo(new Point(x1, y1), new Point(x2, y2), new Point(x3, y3));
    public override void CloseContour() { _context.EndFigure(true); _isOpen = false; }
    public override void Dispose() {
      if (_isOpen) { _context.EndFigure(false); _isOpen = false; }
      _context.Dispose();
      // Passing a null brush is a workaround for https://github.com/AvaloniaUI/Avalonia/issues/1419
      IBrush? brush = Foreground?.ToSolidColorBrush() ?? _canvas.CurrentBrush;
      IPen? pen = null;
      if (_canvas.CurrentStyle == PaintStyle.Stroke) {
        pen = new Pen(brush); brush = null;
      }
      _canvas.DrawingContext.DrawGeometry(brush, pen, _path);
    }
  }
}