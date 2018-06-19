using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class IconPage : ContentPage {
    public IconPage() {
      InitializeComponent();
    }/*
    protected override async void OnAppearing() {
      base.OnAppearing();
      const int size = 40;
      const int count = 10;
      const double π = 3.1415926535897932384626433832795028841971;
      var v = new SKCanvasView();
      v.VerticalOptions = v.HorizontalOptions = LayoutOptions.FillAndExpand;
      v.PaintSurface += PaintSurface;
      Content = v;

      FormsMathView Create(string latex) =>
        new FormsMathView {
          FontSize = size,
          TextAlignment = Rendering.TextAlignment.Center,
          LaTeX = latex
        };

      var a = new AbsoluteLayout();
      Content = a;
      var c = a.Children;
      var o = new Point(a.Width / 2, a.Height / 2);
      FormsMathView v;
      const int r = 100;
      
      for (int i = 0; i < count; i++) {
        v = Create(i.ToString());
        c.Add(v, o);
        var θ = 2 * π * i / count;
        var x = r * Math.Sin(θ);
        var y = -r * Math.Cos(θ);
        await Task.WhenAll(v.TranslateTo(x, y, 400, Easing.SinIn), 
                           v.RotateTo(360 / count * i, 400, null));
      }
      v = Create(@"\;\;\:\,\,\text{C\#} \\ \:\,\, Math");
      c.Add(v, new Rectangle(0, 0, a.Width, a.Height));
    }
    */
    SkiaSharp.SkiaMathPainter painter;
    private void PaintSurface(object sender, SKPaintSurfaceEventArgs e) {
      const int count = 10;
      if (painter == null) painter = new SkiaSharp.SkiaMathPainter(e.Info.Size);
      else painter.Bounds = e.Info.Size;
      var cx = e.Info.Width / 2;
      var cy = e.Info.Height / 2;
      var c = e.Surface.Canvas;
      for (int i = 0; i < count; i++) {
        painter.LaTeX = i.ToString();
        var θ = 360 / count;
        c.RotateDegrees(θ, cx, cy);
        painter.TextAlignment = Rendering.TextAlignment.Center;
        painter.Draw(c);
      }
    }
  }
}