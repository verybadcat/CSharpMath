using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms.Example
{
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class SlidePage : ContentPage {
    SkiaSharp.SkiaMathPainter painter = new SkiaSharp.SkiaMathPainter { LaTeX = @"\text{Press to clear}" };
    bool reset;
    double x, y;

    public SlidePage() => InitializeComponent();

    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      if (reset) { e.Surface.Canvas.Clear(); reset = false; }
      painter.Draw(e.Surface.Canvas, (float)x, (float)y);
      e.Surface.Canvas.DrawRect((float)x + painter.Measure.Value.X, (float)y + painter.Measure.Value.Y, painter.Measure.Value.Width, painter.Measure.Value.Height, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Orange });
    }

    private void Canvas_Touch(object sender, SKTouchEventArgs e) {
      if(e.InContact && e.ActionType == SKTouchAction.Pressed) { reset = true; Canvas.InvalidateSurface(); e.Handled = true; }
    }

    private void SliderX_ValueChanged(object sender, ValueChangedEventArgs e) { x = e.NewValue; Canvas.InvalidateSurface(); }

    private void SliderY_ValueChanged(object sender, ValueChangedEventArgs e) { y = e.NewValue; Canvas.InvalidateSurface(); }
  }
}