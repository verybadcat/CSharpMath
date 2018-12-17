using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaLeakMinProject
{
    public class GLView : SKGLView
    {
        public GLView()
        {

        }
    }

    public class GLMathView : SKGLView
    {
        CSharpMath.SkiaSharp.TextPainter textPainter = new CSharpMath.SkiaSharp.TextPainter() { Text = "$\\int 2x^2 + x - \\frac{a}{b}$", FontSize = 20.0F };
        public GLMathView()
        {

        }
        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
        { 
            textPainter.Draw(e.Surface.Canvas, new System.Drawing.PointF(0.0F, 0.0F), e.BackendRenderTarget.Width);
        }
    }

    public class CanvasView : SKCanvasView
    {
        public CanvasView()
        {

        }
    }
    public class CanvasMathView : SKCanvasView
    {
        CSharpMath.SkiaSharp.TextPainter textPainter = new CSharpMath.SkiaSharp.TextPainter() { Text = "$\\int 2x^2 + x - \\frac{a}{b}$", FontSize = 20.0F };
        public CanvasMathView()
        {

        }
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            textPainter.Draw(e.Surface.Canvas, new System.Drawing.PointF(0.0F, 0.0F), e.Info.Width);
        } 
    }
}