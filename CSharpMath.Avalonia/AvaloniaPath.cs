using CSharpMath.Rendering;
using CSharpMath.Rendering.FrontEnd;
using CSharpMathColor = CSharpMath.Structures.Color;

using Avalonia;
using Avalonia.Media;

namespace CSharpMath.Avalonia {
  public sealed class AvaloniaPath : IPath {
    private readonly AvaloniaCanvas _canvas;

    private readonly PathGeometry _path;

    private StreamGeometryContext _context;
    private bool _isOpen;

    public AvaloniaPath(AvaloniaCanvas canvas) {
      _canvas = canvas;
      _path = new PathGeometry();
    }

    public CSharpMathColor? Foreground { get; set; }

    public void BeginRead(int contourCount) {
      _context = _path.Open();
    }

    public void CloseContour() {
      if (_isOpen) {
        _context.EndFigure(true);
        _isOpen = false;
      }
    }

    public void Curve3(float x1, float y1, float x2, float y2) =>
        _context.QuadraticBezierTo(new Point(x1, y1), new Point(x2, y2));

    public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) =>
        _context.CubicBezierTo(new Point(x1, y1), new Point(x2, y2), new Point(x3, y3));

    public void EndRead() {
      var brush = Foreground?.ToSolidColorBrush() ?? _canvas.CurrentBrush;

      _context.Dispose();
      _canvas.DrawingContext.DrawGeometry(brush, new Pen(brush), _path);
    }

    public void LineTo(float x1, float y1) => _context.LineTo(new Point(x1, y1));

    public void MoveTo(float x0, float y0) {
      if (_isOpen) {
        _context.EndFigure(false);
      }

      _context.BeginFigure(new Point(x0, y0), _canvas.CurrentStyle == PaintStyle.Fill);
    }
  }
}
