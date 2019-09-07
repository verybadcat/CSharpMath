using System.Reflection;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace CSharpMath.NuGetPackageTests.CSharp {
  using Display;
  using MathKeyboard = Forms.MathKeyboard;
  using Rendering;
  using SkiaSharp;
  public partial class MainPage : ContentPage {
    public MainPage() {
      // Basic functionality
      var view = new SKCanvasView { HeightRequest = 225 };
      var keyboard = new MathKeyboard();
      var viewModel = (Rendering.MathKeyboard)typeof(MathKeyboard).GetField("Keyboard", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(keyboard);


      view.EnableTouchEvents = true;
      view.Touch +=
        (sender, e) => {
          if (e.ActionType == SKTouchAction.Pressed)
            viewModel.MoveCaretToPoint(new System.Drawing.PointF(e.Location.X, e.Location.Y));
        };
      keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => {
          var c = e.Surface.Canvas;
          c.Clear();
          MathPainter.DrawDisplay(new MathPainter { TextColor = SKColors.Black }, keyboard.Display, c);
          keyboard.DrawCaret(c, Rendering.CaretShape.IBeam);
        };

      // Input from physical keyboard
      var entry = new Entry { Placeholder = "Enter keystrokes..." };
      entry.TextChanged += (sender, e) => {
        entry.Text = "";
        foreach (var c in e.NewTextValue)
          // The (int) extra conversion seems to be required by Android or a crash occurs
          viewModel.KeyPress((Editor.MathKeyboardInput)c);
      };

      // Debug labels
      var latex = new Label { Text = "LaTeX = " };
      var ranges = new Label { Text = "Ranges = " };
      var index = new Label { Text = "Index = " };
      viewModel.RedrawRequested += (sender, e) => {
        latex.Text = "LaTeX = " + viewModel.LaTeX;
        ranges.Text = "Ranges = " + string.Join(", ", ((ListDisplay<Fonts, Glyph>)viewModel.Display).Displays.Select(x => x.Range));
        index.Text = "Index = " + viewModel.InsertionIndex;
      };

      // Assemble
      Content = new StackLayout { Children = { latex, ranges, index, view, keyboard, entry } };
    }
  }
}