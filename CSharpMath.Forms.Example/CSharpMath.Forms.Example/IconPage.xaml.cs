using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class IconPage : ContentPage {
    public IconPage() {
      InitializeComponent();
      var v = new SKCanvasView();
      v.VerticalOptions = v.HorizontalOptions = LayoutOptions.FillAndExpand;
      v.PaintSurface += PaintSurface;
      Content = v;
    }

    SkiaSharp.SkiaMathPainter painter;
    readonly SKPaint black = new SKPaint { Color = SKColors.Black };
    readonly SKPaint white = new SKPaint { Color = SKColors.White };
    private void PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      const int count = 10; //number of digits in outer circle
      const float r = 100f; //outer circle radius
      const float f = 40f; //font size in points
      const float θ = 360f / count; //angle to rotate when drawing each digit
      if (painter == null) painter = new SkiaSharp.SkiaMathPainter(e.Info.Size) {
        TextAlignment = Rendering.TextAlignment.Center,
        FontSize = f
      };
      else painter.Bounds = e.Info.Size;
      painter.ScrollY = -r; //scroll up
      var cx = e.Info.Width / 2;
      var cy = e.Info.Height / 2;
      var c = e.Surface.Canvas;
      //draw outer circle
      c.DrawCircle(cx, cy, r + f / 2, black);
      painter.TextColor = SKColors.White;
      for (int i = 0; i < count; i++) {
        painter.LaTeX = i.ToString();
        painter.Draw(c);
        c.RotateDegrees(θ, cx, cy);
      }
      //draw inner circle
      c.DrawCircle(cx, cy, r - f / 2, white);
      painter.ScrollY = 0; //reset scroll
      painter.TextColor = SKColors.Black;
      painter.LaTeX = @"\text{\;\;\,C\#}\\Math";
      painter.Draw(c);
    }
  }
}