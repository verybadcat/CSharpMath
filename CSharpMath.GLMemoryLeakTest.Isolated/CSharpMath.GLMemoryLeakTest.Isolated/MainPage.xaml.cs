using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CSharpMath.GLMemoryLeakTest.Isolated {
  public partial class MainPage : ContentPage {
    public MainPage() {
      InitializeComponent();
      var container = new StackLayout();
      Content = container;
      var paint = new global::SkiaSharp.SKPaint { Color = global::SkiaSharp.SKColors.Blue };
      for (var i = 1; i < 30; i++) {
        container.Children.Clear();
        for (var j = 1; j < 150; j++) {
          var gl = new global::SkiaSharp.Views.Forms.SKGLView();
          gl.PaintSurface += (sender, e) => {
            var c = e.Surface.Canvas;
            using (var p = new global::SkiaSharp.SKPath()) {
              p.MoveTo(0, 0);
              p.ArcTo(5, 5, 0, 10, 5);
              p.ArcTo(5, 5, 10, 10, 5);
              p.ArcTo(5, 5, 10, 0, 5);
              p.ArcTo(5, 5, 0, 0, 5);
              p.Close();
              c.DrawPath(p, paint);
            }
          };
          container.Children.Add(gl);
        }
      }
    }
  }
}
