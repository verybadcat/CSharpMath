using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LayoutPage : ContentPage {
    readonly SkiaSharp.TextPainter painter = new SkiaSharp.TextPainter { LaTeX =
      @"Here are some text.
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
Was added in 0.1.0-pre4; working in 0.1.0-pre5; fully tested in 0.1.0-pre6. \[\frac{Display}{maths} \sqrt\text\mathtt{\ at\ the\ end}^\mathbf{are\ now\ incuded\ in\ Measure!} \]"
.Remove(0) + @"display maths skipping: here is abovedisplayskip\[Maths\]shortskip"
.Remove(0) + @"Maths on the baseline $\rightarrow\int^6_4 x dx$"
.Remove(0) + @"See how the allowed width affects TextPainter$$\sum\int^3_2x\ dx$$Text after maths"
    };
    bool reset, drawOneLine;
    float x, y, w;
    public LayoutPage() => InitializeComponent();
    private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      if (reset) { e.Surface.Canvas.Clear(); reset = false; }
      e.Surface.Canvas.Clear();
      if (drawOneLine) {
        var measure = painter.Measure(float.PositiveInfinity) ?? throw new Structures.InvalidCodePathException("Invalid LaTeX");
        e.Surface.Canvas.DrawRect(x, y,
          measure.Width, measure.Height, new global::SkiaSharp.SKPaint {
            Color = global::SkiaSharp.SKColors.Orange
          });
        painter.DrawOneLine(e.Surface.Canvas, x, y);
      } else {
        var measure = painter.Measure(w) ?? throw new Structures.InvalidCodePathException("Invalid LaTeX");
        e.Surface.Canvas.DrawRect(x, y,
          measure.Width, measure.Height, new global::SkiaSharp.SKPaint {
            Color = global::SkiaSharp.SKColors.Orange
          });
        measure = painter.Display?.Frame() ?? throw new Structures.InvalidCodePathException("Invalid LaTeX");
        e.Surface.Canvas.DrawRect(x, y,
          measure.Width, measure.Height, new global::SkiaSharp.SKPaint {
            Color = global::SkiaSharp.SKColors.Green,
            IsStroke = true
          });
        measure = painter._absoluteXCoordDisplay.DisplayBounds();
        e.Surface.Canvas.DrawRect(x, y,
          measure.Width, measure.Height, new global::SkiaSharp.SKPaint {
            Color = global::SkiaSharp.SKColors.Red,
            IsStroke = true
          });
        measure = painter._relativeXCoordDisplay.DisplayBounds();
        e.Surface.Canvas.DrawRect(x, y,
          measure.Width, measure.Height, new global::SkiaSharp.SKPaint {
            Color = global::SkiaSharp.SKColors.Blue,
            IsStroke = true
          });
        painter.Draw(e.Surface.Canvas, new System.Drawing.PointF(x, y), w);
      }
    }
    private void OneLineButton_Clicked(object sender, System.EventArgs e) {
      OneLineButton.BackgroundColor = (drawOneLine =
        OneLineButton.BackgroundColor == Color.Default) ? Color.Orange : Color.Default;
      Canvas.InvalidateSurface();
    }
    private void Slider_ValueChanged(object sender, ValueChangedEventArgs e) {
      (sender == SliderX ? ref x : ref (sender == SliderY ? ref y : ref w)) =
        (float)e.NewValue;
      (sender == SliderX ? SliderXValue : sender == SliderY ? SliderYValue : SliderWValue)
        .Text = e.NewValue.ToString(" 0000.00;-0000.00");
      Canvas.InvalidateSurface();
    }
    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);
      SliderX.Minimum = SliderY.Minimum = SliderW.Minimum = -9999.99;
      SliderX.Maximum = SliderY.Maximum = SliderW.Maximum = 9999.99;
    }
  }
}