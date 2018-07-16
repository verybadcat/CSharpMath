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
      e.Surface.Canvas.DrawCircle(0f, 0f, 100f, new global::SkiaSharp.SKPaint { Color = new global::SkiaSharp.SKColor(255, 0, 0) });
      painter.Draw(e.Surface.Canvas, (float)x, (float)y);
      e.Surface.Canvas.DrawCircle(300f, 0f, 100f, new global::SkiaSharp.SKPaint { Color = new global::SkiaSharp.SKColor(255, 255, 0) });
    }

    private void Canvas_Touch(object sender, SKTouchEventArgs e) {
      if(e.InContact && e.ActionType == SKTouchAction.Pressed) { reset = true; Canvas.InvalidateSurface(); e.Handled = true; }
    }

    private void SliderX_ValueChanged(object sender, ValueChangedEventArgs e) { x = e.NewValue; Canvas.InvalidateSurface(); }

    private void SliderY_ValueChanged(object sender, ValueChangedEventArgs e) { y = e.NewValue; Canvas.InvalidateSurface(); }
  }
}