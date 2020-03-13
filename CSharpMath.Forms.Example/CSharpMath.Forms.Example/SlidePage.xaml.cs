using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class SlidePage : ContentPage {
    readonly SkiaSharp.MathPainter painter = new SkiaSharp.MathPainter { LaTeX = @"\text{Press to clear}" };
    bool reset;
    double x, y;

    public SlidePage() => InitializeComponent();

    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      if (reset) { e.Surface.Canvas.Clear(); reset = false; }
      var measure = painter.Measure().Value;
      e.Surface.Canvas.DrawRect
        ((float)x + measure.X, (float)y + measure.Y, measure.Width, measure.Height,
          new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Orange });
      painter.Draw(e.Surface.Canvas, (float)x, (float)y);
    }

    private void Canvas_Touch(object sender, SKTouchEventArgs e) {
      if (e.InContact && e.ActionType == SKTouchAction.Pressed)
        { reset = true; Canvas.InvalidateSurface(); e.Handled = true; }
    }

    private void SliderX_ValueChanged(object sender, ValueChangedEventArgs e)
      { x = e.NewValue; Canvas.InvalidateSurface(); }

    private void SliderY_ValueChanged(object sender, ValueChangedEventArgs e)
      { y = e.NewValue; Canvas.InvalidateSurface(); }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);
      //+1 to avoid Maximum == Minimum == 0 (which throws) but allow for sliding
      (SliderX.Maximum, SliderY.Maximum) = (Canvas.CanvasSize.Width + 1, Canvas.CanvasSize.Height + 1);
      SliderX.Minimum = SliderY.Minimum = 0;
    }
  }
}