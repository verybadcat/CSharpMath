using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CSharpMath.Forms.EditorUnitTests {
  public partial class MainPage : ContentPage {
    public MainPage() {
      const float y = 40;
      InitializeComponent();
      var canvas = new global::SkiaSharp.Views.Forms.SKCanvasView();
      var painter = new SkiaSharp.MathPainter { LaTeX = @"\frac32" };
      canvas.PaintSurface += (sender, e) => {
        var c = e.Surface.Canvas;
        painter.Draw(c, 0, y);
        c.DrawPoint(0, )
      };
      Content = canvas;
    }
  }
}
