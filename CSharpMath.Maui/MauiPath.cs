using Microsoft.Maui.Graphics;
using CSharpMathColor = System.Drawing.Color;

namespace CSharpMath.Maui {
  public sealed class MauiPath(MauiCanvas owner) : Rendering.FrontEnd.Path {
    public override CSharpMathColor? Foreground { get; set; }

    private readonly PathF _path = new();
    public override void MoveTo(float x0, float y0) { _path.MoveTo(x0, y0); }
    public override void LineTo(float x1, float y1) => _path.LineTo(x1, y1);
    public override void Curve3(float x1, float y1, float x2, float y2) =>
      _path.QuadTo(x1, y1, x2, y2);
    public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) =>
      _path.CurveTo(x1, y1, x2, y2, x3, y3);
    public override void CloseContour() { if (_path.Count > 0) _path.Close(); } // Throws IndexOutOfRangeException if we don't check _path.Count 
    public override void Dispose() {
      owner.Canvas.SaveState();
      var c = (Foreground ?? owner.CurrentColor ?? owner.DefaultColor).ToMauiColor();
      if (owner.CurrentStyle == Rendering.FrontEnd.PaintStyle.Fill)
        { owner.Canvas.FillColor = c; owner.Canvas.StrokeSize = 0; owner.Canvas.FillPath(_path); }
      else { owner.Canvas.StrokeColor = c; owner.Canvas.StrokeSize = 2; owner.Canvas.DrawPath(_path); }
      owner.Canvas.RestoreState();
      _path.Dispose();
    }
  }
}
