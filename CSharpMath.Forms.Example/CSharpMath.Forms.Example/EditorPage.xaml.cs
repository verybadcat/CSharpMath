using System.Linq;
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
      var keyboard = new MathKeyboard(Rendering.FrontEnd.PainterConstants.LargerFontSize);
      var viewModel = keyboard.Keyboard;
      viewModel.BindDisplay(view, new SkiaSharp.MathPainter {
        TextColor = SKColors.Black
      }, new SKColor(0, 0, 0, 153));

      // Input from physical keyboard
      var entry = new Entry {
        Placeholder = "Enter keystrokes...",
        HorizontalOptions = LayoutOptions.FillAndExpand
      };
      entry.TextChanged += (sender, e) => {
        entry.Text = "";
        foreach (var c in e.NewTextValue)
          // The (int) extra conversion seems to be required by Android or a crash occurs
          viewModel.KeyPress((Editor.MathKeyboardInput)(int)c);
      };

      // Evaluation
      var output = new MathView { FontSize = 32, EnableTouchEvents = true, EnablePanning = true };
      keyboard.Keyboard.ReturnPressed += delegate {
        output.LaTeX = Evaluation.Interpret(keyboard.Keyboard.MathList);
      };

      // Debug labels
      var latex = new Label { Text = "LaTeX = " };
      var atomTypes = new Label { Text = "Atom Types = " };
      var ranges = new Label { Text = "Ranges = " };
      var index = new Label { Text = "Index = " };
      viewModel.RedrawRequested += (sender, e) => Device.BeginInvokeOnMainThread(() => {
        latex.Text = "LaTeX = " + viewModel.LaTeX;
        atomTypes.Text = "Atom Types = " + string.Join
          (", ", viewModel.MathList.Select(x => x.GetType().Name));
        ranges.Text = "Ranges = " + string.Join
          (", ", (viewModel.Display ?? throw new Structures.InvalidCodePathException("Invalid LaTeX"))
                 .Displays.Select(x => x.Range));
        index.Text = "Index = " + viewModel.InsertionIndex;
      });

      static View GridItem(int row, int col, View view) {
        Grid.SetRow(view, row);
        Grid.SetColumn(view, col);
        return view;
      }
      // Assemble
      Content = new Grid {
        RowDefinitions = {
          new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
          new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) },
          new RowDefinition { Height = 1 },
          new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) },
          new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
        },
        Children = {
          GridItem(0, 0, new ScrollView {
            Content = new StackLayout {
              Children = { latex, atomTypes, ranges, index }
            }
          }),
          GridItem(1, 0, view),
          GridItem(2, 0, new BoxView { Color = Color.Gray }),
          GridItem(3, 0, output),
          GridItem(4, 0, new StackLayout {
            Children = {
              keyboard,
              new StackLayout {
                Orientation = StackOrientation.Horizontal,
                Children = {
                  entry,
                  new Button {
                    Text = "Reset answer pan",
                    Command = new Command(() => output.DisplacementX = output.DisplacementY = 0)
                  }
                }
              }
            }
          })
        }
      };
    }
  }
}