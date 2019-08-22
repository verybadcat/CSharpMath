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
    const string Text = "Enter  keystrokes...";
    static readonly int CursorPosition = Text.IndexOf(' ') + 1;
    public EditorView() {

      var view = new SKCanvasView { HeightRequest = 225 };
      var keyboard = new MathKeyboard();
      keyboard.BindDisplay(view, new SkiaSharp.MathPainter {
        TextColor = SKColors.Black
      }, new SKColor(0, 0, 0, 153));
      var entry = new Entry { Text = Text };
      entry.Focused += (sender, e) => entry.CursorPosition = CursorPosition;
      entry.PropertyChanged += (sender, e) => {
        switch (e.PropertyName) {
          case nameof(Entry.Text):
            var input = entry.Text.Split(' ')[1];
            foreach (var c in input)
              keyboard.KeyPress((Editor.MathKeyboardInput)(int)c);
            entry.Text = Text;
            // Setting text also sets the cursor to the end
            entry.CursorPosition = CursorPosition;
            break;
          case nameof(Entry.CursorPosition):
            var times = entry.CursorPosition - CursorPosition;
            System.Diagnostics.Debug.WriteLine("[1] " + times + " " + entry.CursorPosition);
            if (times == 0) return; // Avoid infinite loop by assignment below
            System.Diagnostics.Debug.WriteLine("[2] " + times + " " + entry.CursorPosition);
            while (times != 0) {
              if (times < 0) {
                keyboard.KeyPress(Editor.MathKeyboardInput.Left);
                times++;
              } else {
                keyboard.KeyPress(Editor.MathKeyboardInput.Right);
                times--;
              }
            }
            System.Diagnostics.Debug.WriteLine("[3] " + times + " " + entry.CursorPosition);
            entry.CursorPosition = CursorPosition;
            break;
        }
      };
      Content = new StackLayout { Children = { view, keyboard, entry } };
    }
  }
}