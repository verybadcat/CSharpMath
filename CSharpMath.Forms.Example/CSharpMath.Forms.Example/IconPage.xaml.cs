using Xamarin.Forms;
using Xamarin.Forms.Xaml;
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

    private void PaintSurface(object sender, SKPaintSurfaceEventArgs e) =>
      Rendering.Tests.DrawIcon.Draw(e.Surface.Canvas);
  }
}