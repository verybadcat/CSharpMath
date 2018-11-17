using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CSharpMath.Forms.EditorUnitTests {
  using Editor;
  using Rendering;
  static class Temp {
    public static T To<T>(this object source) => (T)source;
  }
  public partial class MainPage : ContentPage {
    public MainPage() {
      const float y = 40;
      const float canvasInvert = -1;
      InitializeComponent();
      var canvas = new global::SkiaSharp.Views.Forms.SKCanvasView();
      var painter = new SkiaSharp.MathPainter { LaTeX = @"999999" };
      canvas.PaintSurface += (sender, e) => {
        var c = e.Surface.Canvas;
        c.Scale(10, 10);
        painter.Draw(c, 0, y);
        for (int i = 0; i < painter.Display.To<Display.ListDisplay<Fonts, Glyph>>().Displays[0].To<Display.TextLineDisplay<Fonts, Glyph>>().Runs[0].Run.Length; i++)
          if (painter.Display.PointForIndex(TypesettingContext.Instance, MathListIndex.Level0Index(i)) is System.Drawing.PointF p)
            c.DrawCircle(p.X, canvasInvert * p.Y, 1, new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Red });
          else System.Diagnostics.Debugger.Break();
      };
      Content = canvas;
    }
  }
}
