using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LayoutPage : ContentPage {
    SkiaSharp.TextPainter painter = new SkiaSharp.TextPainter { Text = @"Here are some text.
This text is made to be long enough to have the TextPainter of CSharpMath add a line break to this text automatically.
To demonstrate the capabilities of the TextPainter,
here are some math content:
First, a fraction in inline mode: $\frac34$
Next, a summation in inline mode: $\sum_{i=0}^3i^i$
Then, a summation in display mode: $$\sum_{i=0}^3i^i$$
After that, an integral in display mode: $$\int^6_{-56}x\ dx$$
Finally, an escaped dollar sign \$ that represents the start/end of math mode when it is unescaped.
Colors can be achieved via \backslash color{color}{content}, or \backslash \textit{color}{content},
where \textit{color} stands for one of the LaTeX standard colors.
\red{Colored text in text mode are able to automatically break up when spaces are inside the colored text, which the equivalent in math mode cannot do.}
\textbf{Styled} \texttt{text} can be achieved via the LaTeX styling commands.
The SkiaSharp version of this is located at CSharpMath.SkiaSharp.TextPainter;
and the Xamarin.Forms version of this is located at CSharpMath.Forms.TextView.
Was added in 0.1.0-pre4; working in 0.1.0-pre5; fully tested in 0.1.0-pre6. \[\frac{Display}{maths} \sqrt\text\mathtt{\ at\ the\ end}^\mathbf{are\ now\ incuded\ in\ Measure!} \]" };
    bool reset;
    double x, y, w;

    public LayoutPage() => InitializeComponent();

    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      if (reset) { e.Surface.Canvas.Clear(); reset = false; }
      var measure = painter.Measure((float)w).Value;
      e.Surface.Canvas.Clear();
      e.Surface.Canvas.DrawRect((float)x + measure.X, (float)y + measure.Y, measure.Width, measure.Height, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Orange });
      /*measure = painter._absoluteXCoordDisplay.ComputeDisplayBounds();
      e.Surface.Canvas.DrawRect((float)x + measure.X, (float)y + measure.Y, measure.Width, measure.Height, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Red, IsStroke = true });
      measure = painter._relativeXCoordDisplay.ComputeDisplayBounds();
      e.Surface.Canvas.DrawRect((float)x + measure.X, (float)y + measure.Y, measure.Width, measure.Height, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Blue, IsStroke = true });
      */painter.Draw(e.Surface.Canvas, new System.Drawing.PointF((float)x, (float)y), (float)w);
    }

    //Add Epsilon to prevent SliderW.Minimum == SiderW.Maximum == 0
    private void SliderX_ValueChanged(object sender, ValueChangedEventArgs e) { x = e.NewValue; SliderW.Maximum = SliderX.Maximum - x + float.Epsilon; Canvas.InvalidateSurface(); }

    private void SliderY_ValueChanged(object sender, ValueChangedEventArgs e) { y = e.NewValue; Canvas.InvalidateSurface(); }

    private void SliderW_ValueChanged(object sender, ValueChangedEventArgs e) { w = e.NewValue; Canvas.InvalidateSurface(); }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);
      //+1 to avoid Maximum == Minimum == 0 (which throws) but allow for sliding
      //Add Epsilon to prevent SliderW.Minimum == SiderW.Maximum == 0
      (SliderX.Maximum, SliderY.Maximum, SliderW.Maximum) = (Canvas.CanvasSize.Width + 1, Canvas.CanvasSize.Height + 1, Canvas.CanvasSize.Width + 1 - SliderX.Value + float.Epsilon);
      SliderX.Minimum = SliderY.Minimum = SliderW.Minimum = 0;
    }
  }
}