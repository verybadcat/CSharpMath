// Adapted after https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.SkiaSharp/SkiaCanvas.cs

using System.Drawing;
using CSharpMath.Rendering.FrontEnd;
using VectSharp;

namespace CSharpMath.VectSharp
{
    public sealed class VectSharpCanvas : ICanvas
    {
        public VectSharpCanvas(Page canvas, bool antiAlias)
        {
            Canvas = canvas;
            this._isAntialias = antiAlias;
        }
        public Page Canvas { get; }
        public float Width => (float)Canvas.Width;
        public float Height => (float)Canvas.Height;
        public Color DefaultColor { get; set; }
        public Color? CurrentColor { get; set; }
        public PaintStyle CurrentStyle { get; set; }

        private readonly bool _isAntialias;

        // Canvas methods
        public void StrokeRect(float left, float top, float width, float height)
        {
            this.Canvas.Graphics.StrokeRectangle(left, top, width, height, (this.CurrentColor ?? this.DefaultColor).ToNative());
        }
          
        public void FillRect(float left, float top, float width, float height)
        {
            this.Canvas.Graphics.FillRectangle(left, top, width, height, (this.CurrentColor ?? this.DefaultColor).ToNative());
        }

        public void DrawLine(float x1, float y1, float x2, float y2, float lineThickness)
        {
            if (CurrentStyle == PaintStyle.Fill)
            {
                this.Canvas.Graphics.StrokePath(new GraphicsPath().MoveTo(x1, y1).LineTo(x2, y2), (this.CurrentColor ?? this.DefaultColor).ToNative(), lineThickness);
            }
            else
            {
                this.StrokeLineOutline(x1, y1, x2, y2, lineThickness);
            }
        }
        public void Save() => this.Canvas.Graphics.Save();
        public void Translate(float dx, float dy) => this.Canvas.Graphics.Translate(dx, dy);
        public void Scale(float sx, float sy) => this.Canvas.Graphics.Scale(sx, sy);
        public void Restore() => this.Canvas.Graphics.Restore();
        public Path StartNewPath() => new VectSharpPath(this);
    }
}
