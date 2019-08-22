using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class EditorPage : ContentPage {
    public EditorPage() {
      InitializeComponent();
      Content = new EditorView();
    }
  }

  public class EditorView : ContentView {
    public EditorView() {

      var view = new SKCanvasView { HeightRequest = 225 };
      var keyboard = new MathKeyboard();
      keyboard.ViewModel.BindDisplay(view, new SkiaSharp.MathPainter {
        TextColor = SKColors.Black
      }, new SKColor(0, 0, 0, 153));
      var entry = new Entry { Placeholder = "Enter keystrokes..." };
      entry.TextChanged += (sender, e) => {
          entry.Text = "";
          foreach (var c in e.NewTextValue)
            keyboard.ViewModel.KeyPress((Editor.MathKeyboardInput)(int)c);
      };
      Content = new StackLayout { Children = { view, keyboard, entry } };
    }
  }
}