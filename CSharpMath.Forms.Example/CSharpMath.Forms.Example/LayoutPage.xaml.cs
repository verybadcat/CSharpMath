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
    SkiaSharp.TextPainter painter = new SkiaSharp.TextPainter { Text = @"Here are some text. This text is made to be long enough to have the TextPainter of CSharpMath (hopefully) add a line break to this text automatically. To demonstrate the capabilities of the TextPainter, here are some math content: First, a fraction in inline mode: $\frac34$ Next, a summation in inline mode: $\sum_{i=0}^3i^i$ Then, a summation in display mode: $$\sum_{i=0}^3i^i$$ (ah, bugs.) After that, an integral in display mode: $$\int^6_{-56}x\ dx$$ Finally, an escaped dollar sign \$ that represents the start/end of math mode when it is unescaped. Even though colours are currently unsupported, it can be done via math mode with the \\color command with the help of the \\text command. It looks like this: $\color{#F00}{\text{some red text}}$, which is nearly indistinguishable from non-math mode aside from not being able to automatically break up when spaces are inside the coloured text. The SkiaSharp version of this is located at CSharpMath.SkiaSharp.TextPainter; and the Xamarin.Forms version of this is located at CSharpMath.Forms.TextView. Was added in 0.1.0-pre4; working in 0.1.0-pre5." };
    bool reset;
    double x, y, w;

    public LayoutPage() => InitializeComponent();

    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      if (reset) { e.Surface.Canvas.Clear(); reset = false; }
      var measure = painter.Measure((float)w).Value;
      e.Surface.Canvas.Clear();
      e.Surface.Canvas.DrawRect((float)x + measure.X, (float)y + measure.Y, measure.Width, measure.Height, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Orange });
      painter.Draw(e.Surface.Canvas, new System.Drawing.PointF((float)x, (float)y), (float)w);
    }

    private void SliderX_ValueChanged(object sender, ValueChangedEventArgs e) { x = e.NewValue; SliderW.Maximum = SliderX.Maximum - x; Canvas.InvalidateSurface(); }

    private void SliderY_ValueChanged(object sender, ValueChangedEventArgs e) { y = e.NewValue; Canvas.InvalidateSurface(); }

    private void SliderW_ValueChanged(object sender, ValueChangedEventArgs e) { w = e.NewValue; Canvas.InvalidateSurface(); }
  }
}