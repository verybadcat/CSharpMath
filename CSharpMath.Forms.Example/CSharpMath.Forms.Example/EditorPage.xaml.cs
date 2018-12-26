using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class EditorPage : ContentPage {
    public EditorPage() {
      InitializeComponent();
      var view = new global::SkiaSharp.Views.Forms.SKCanvasView { WidthRequest = 320, HeightRequest = 225 };
      var painter = new SkiaSharp.MathPainter { TextColor = global::SkiaSharp.SKColors.Black };
      var keyboard = new MathKeyboard();
      keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => SkiaSharp.MathPainter.DrawDisplay(painter, keyboard.Display, e.Surface.Canvas);
      Content = new StackLayout {
        Children = {
          view,
          keyboard,
          new BoxView {
            HeightRequest = 50, WidthRequest = 50, Color = Color.Black
          }
        }
      };
    }
  }
}