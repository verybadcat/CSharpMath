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
    public EditorView() {
      var view = new SKCanvasView();
      var keyboard = new MathKeyboard();
      keyboard.BindDisplay(view, new SkiaSharp.MathPainter {
        TextColor = global::SkiaSharp.SKColors.Black
      });
      Content = new StackLayout { Children = { view, keyboard } };
    }
  }
}