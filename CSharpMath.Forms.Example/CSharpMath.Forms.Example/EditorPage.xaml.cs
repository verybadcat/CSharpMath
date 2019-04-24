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
        (sender, e) => {
          e.Surface.Canvas.Clear();
          SkiaSharp.MathPainter.DrawDisplay(painter, keyboard.Display, e.Surface.Canvas);
          keyboard.DrawCaret(e.Surface.Canvas, Rendering.CaretShape.UpArrow);
        };
      Content = new StackLayout { Children = { view, keyboard } };
    }
  }
}