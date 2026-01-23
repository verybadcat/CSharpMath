namespace CSharpMath.Maui.Example {
  /// <summary>
  /// Credits to https://github.com/sadqiang
  /// at https://github.com/verybadcat/CSharpMath/issues/27
  /// </summary>
  public partial class ClockPage : ContentPage, IDrawable {
    readonly string[] labels = {
      // Four 4s make 1 to 12 using different operations
      @"$\frac{44+4}{4}$",
      @"$\frac{44}{44}$",
      @"$\frac{4}{4}+\frac{4}{4}$",
      @"$\frac{4+4+4}{4}$",
      @"$4+\frac{4-4}{4}$",
      @"$4+4^{4-4}$",
      @"$4+\frac{4+4}{4}$",
      @"$\frac{44}{4}-4$",
      @"$\sqrt{4}^{4-\frac{4}{4}}$",
      @"$\:\:(4-\frac{4}{4})^{\sqrt{4}}$",
      @"$\frac{44-4}{4}$",
      @"$\frac{4!}{\sqrt{4}}-\frac{4}{4}$"
    };
    public ClockPage() {
      InitializeComponent();

      Dispatcher.StartTimer(TimeSpan.FromMilliseconds(20), () => {
        canvasView.Invalidate();
        return true;
      });
      canvasView.Drawable = this;
    }
    public void Draw(ICanvas canvas, RectF dirtyRect) {
      canvas.SaveState();
      canvas.FillColor = Colors.CornflowerBlue;
      canvas.FillRectangle(dirtyRect);
      canvas.RestoreState();
      canvas.Translate(dirtyRect.Width / 2, dirtyRect.Height / 2);
      var minWidthHeight = Math.Min(dirtyRect.Width, dirtyRect.Height);
      canvas.Scale(minWidthHeight / 210f, minWidthHeight / 210f);
      canvas.SaveState();
      canvas.FillColor = Colors.Black;
      canvas.FillCircle(0, 0, 100);
      canvas.RestoreState();
      var painter = new TextPainter { FontSize = 8, TextColor = Colors.White };
      for (int i = 0; i < 60; i++) {
        // Dots
        canvas.SaveState();
        canvas.Rotate(6 * i);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(0, -90, i % 5 == 0 ? 4 : 2);
        canvas.RestoreState();
        // Maths
        if (i % 5 == 0) {
          painter.LaTeX = labels[i / 5];
          if (!(painter.Measure(dirtyRect.Width) is { } measure))
            throw new Atom.InvalidCodePathException("Invalid LaTeX");
          var θ = (90 - 6 * i) / 180f * MathF.PI;
          var (sinθ, cosθ) = MathF.SinCos(θ);
          painter.Draw((canvas, dirtyRect.Size),
            new System.Drawing.PointF(75 * cosθ - measure.Width / 2,
              -75 * sinθ - measure.Height / 2),
            float.PositiveInfinity);
        }
      }
      var dateTime = DateTime.Now;
      // H
      canvas.SaveState();
      canvas.Rotate(30 * dateTime.Hour + dateTime.Minute / 2f);
      canvas.StrokeColor = Colors.White;
      canvas.StrokeLineCap = LineCap.Round;
      canvas.StrokeSize = 12;
      canvas.DrawLine(0, 0, 0, -50);
      canvas.RestoreState();
      // M
      canvas.SaveState();
      canvas.Rotate(6 * dateTime.Minute + dateTime.Second / 10f);
      canvas.StrokeColor = Colors.White;
      canvas.StrokeLineCap = LineCap.Round;
      canvas.StrokeSize = 6;
      canvas.DrawLine(0, 0, 0, -65);
      canvas.RestoreState();
      // S
      canvas.SaveState();
      canvas.Rotate(6f * (dateTime.Second + dateTime.Millisecond / 1000f));
      canvas.StrokeColor = Colors.Red;
      canvas.StrokeLineCap = LineCap.Round;
      canvas.StrokeSize = 2;
      canvas.DrawLine(0, 0, 0, -75);
      canvas.RestoreState();
    }
  }
}