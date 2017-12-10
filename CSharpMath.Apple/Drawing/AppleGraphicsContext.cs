using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Apple.Drawing {
  public class AppleGraphicsContext : IGraphicsContext {
    public CGContext CgContext { get; set; }
    public void DrawLine(float x1, float y1, float x2, float y2) {
      CgContext.MoveTo(x1, y1);
      CgContext.AddLineToPoint(x2, y2);
      CgContext.DrawPath(CGPathDrawingMode.Stroke);
    }

    public void DrawTextAtPoint(string text, PointF point, float maxWidth = float.NaN) {

    }

    public void RestoreState() {
      CgContext.RestoreState();
    }

    public void SaveState() {
      CgContext.SaveState();
    }

    public void SetTextPosition(PointF position) {
      throw new NotImplementedException();
    }

    public void Translate(PointF dxy) {
      CgContext.TranslateCTM(dxy.X, dxy.Y);
    }
  }
}
