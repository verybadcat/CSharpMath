// Adapted after https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.SkiaSharp/SkiaPath.cs

using System.Drawing;
using VectSharp;

namespace CSharpMath.VectSharp
{
    public sealed class VectSharpPath : Rendering.FrontEnd.Path
    {
        public VectSharpPath(VectSharpCanvas owner) => _owner = owner;
        public override Color? Foreground { get; set; }
        private readonly VectSharpCanvas _owner;
        private readonly GraphicsPath _path = new GraphicsPath();
        public override void MoveTo(float x0, float y0) { _path.Close(); _path.MoveTo(x0, y0); }
        public override void LineTo(float x1, float y1) => _path.LineTo(x1, y1);
        public override void Curve3(float x1, float y1, float x2, float y2)
        {
            _path.QuadraticBezierTo(x1, y1, x2, y2);
        }
          
        public override void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _path.CubicBezierTo(x1, y1, x2, y2, x3, y3);
        }
          
        public override void CloseContour() => _path.Close();
        public override void Dispose()
        {
            if (_owner.CurrentStyle == Rendering.FrontEnd.PaintStyle.Fill)
            {
                _owner.Canvas.Graphics.FillPath(this._path, (this.Foreground ?? _owner.CurrentColor ?? _owner.DefaultColor).ToNative());
            }
            else
            {
                _owner.Canvas.Graphics.StrokePath(this._path, (this.Foreground ?? _owner.CurrentColor ?? _owner.DefaultColor).ToNative());
            }
        }
    }
}
