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
      var painter = new SkiaSharp.MathPainter { TextColor = global::SkiaSharp.SKColors.Black };
      keyboard.BindTo(view, painter);
      Content = new StackLayout { Children = { view, keyboard } };
    }
  }
}