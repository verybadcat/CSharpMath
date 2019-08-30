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
      // Basic functionality
      var view = new SKCanvasView { HeightRequest = 225 };
      var keyboard = new MathKeyboard();
      keyboard.ViewModel.BindDisplay(view, new SkiaSharp.MathPainter {
        TextColor = SKColors.Black
      }, new SKColor(0, 0, 0, 153));

      // Input from physical keyboard
      var entry = new Entry { Placeholder = "Enter keystrokes..." };
      entry.TextChanged += (sender, e) => {
          entry.Text = "";
          foreach (var c in e.NewTextValue)
            // The (int) extra conversion seems to be required by Android or a crash occurs
            keyboard.ViewModel.KeyPress((Editor.MathKeyboardInput)(int)c);
      };

      // Debug labels
      var latex = new Label { Text = "LaTeX = " };
      var index = new Label { Text = "Index = " };
      keyboard.ViewModel.RedrawRequested += (sender, e) => {
        latex.Text = "LaTeX = " + keyboard.ViewModel.LaTeX;
        index.Text = "Index = " + keyboard.ViewModel.InsertionIndex;
      };

      // Assemble
      Content = new StackLayout { Children = { latex, index, view, keyboard, entry } };
    }
  }
}