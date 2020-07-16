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
      Content = new StackLayout {
        Spacing = 0,
        Children = {
          new MathView { LaTeX = @"\frac23", BackgroundColor = Color.Red },
          new MathView { LaTeX = @"\frac46", BackgroundColor = Color.Green },
          new MathView { LaTeX = @"\frac69", BackgroundColor = Color.Blue },
        }
      };
    }

    private void PaintSurface(object sender, SKPaintSurfaceEventArgs e) =>
      Rendering.Tests.DrawIcon.Draw(e.Surface.Canvas);
  }
}