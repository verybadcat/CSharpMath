using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Editor;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class EditorPage : ContentPage {

    public EditorPage() {
      InitializeComponent();
      this.Content = new EditorView();
    }

  }

  public class EditorView : ContentView {
    private MathKeyboard keyboard;

    public EditorView() {
      keyboard = new MathKeyboard();

      var view = new SKCanvasView { WidthRequest = 320, HeightRequest = 225, EnableTouchEvents = true };
      view.Touch +=
        (sender, e) => {
          if (e.ActionType == SKTouchAction.Pressed) {
            keyboard.Tap(new System.Drawing.PointF(e.Location.X, e.Location.Y));
          }
        };

      var painter = new SkiaSharp.MathPainter { TextColor = global::SkiaSharp.SKColors.Black };
      keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => {
          e.Surface.Canvas.Clear();
          SkiaSharp.MathPainter.DrawDisplay(painter, keyboard.Display, e.Surface.Canvas);
          keyboard.DrawCaret(e.Surface.Canvas, Rendering.CaretShape.IBeam);
        };
      Content = new StackLayout { Children = { view, keyboard } };
    }
  }
}