using System;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.SkiaSharp;
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
    public MathPainter OutputMathPainter = new MathPainter { TextColor = SKColors.Black };
    MathKeyboard keyboard = new MathKeyboard(Rendering.FrontEnd.PainterConstants.LargerFontSize);
    public EditorView() {
      // Basic functionality
      var view = new SKCanvasView { HeightRequest = 225 };
      var viewModel = keyboard.Keyboard;
      viewModel.BindDisplay(view, OutputMathPainter, new SKColor(0, 0, 0, 153));

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
                  new Button { Text = "Change appearance", Command = new Command(ChangeAppearance) },
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
    int CurrentThemeIndex = 0;
    public void ChangeAppearance() {
      CurrentThemeIndex = (CurrentThemeIndex + 1) % Themes.Count;
      Themes[CurrentThemeIndex].Invoke();
      keyboard.Keyboard.InsertionIndex = keyboard.Keyboard.InsertionIndex; // Hack to redraw placeholders in the output.
    }
    IList<Action> Themes => new Action[] {
      () => { // This theme is the default. For a round-trip through the themes we need to set them again:
        OutputMathPainter.TextColor = SKColors.Black;
        Atom.LaTeXSettings.PlaceholderBlinks = false;
        Atom.LaTeXSettings.PlaceholderActiveColor = null;
        Atom.LaTeXSettings.PlaceholderRestingColor = null;
        Atom.LaTeXSettings.PlaceholderActiveNucleus = "■";
        Atom.LaTeXSettings.PlaceholderRestingNucleus = "□";
        keyboard.SetButtonsTextColor(Color.Black);
        keyboard.SetClearButtonImageSource("Controls/ImageSourceMathInputButtons/recyclebin.png");
      },
      () => {
        UseMyCustomizedPlaceholderAppearance();
        keyboard.SetButtonsTextColor(Color.Black); // Placeholder appearance on the keys is the same as in the output by default.
        keyboard.SetClearButtonImageSource("Controls/ImageSourceMathInputButtons/metaltrashcan.png");
      },
      () => {
        Atom.LaTeXSettings.PlaceholderBlinks = true;
        OutputMathPainter.TextColor = SKColors.DarkGreen;
        UseMyCustomizedPlaceholderAppearance();
        // If you'd like to use different keyboard colors than output colors and you specified a placeholder color,
        // probably you'll not want to use the same placeholder color on the keyboard:
        keyboard.SetButtonsTextColor(Color.Brown, CalculateMyPlaceholderRestingColorFromSurroundingTextColor(Color.Brown));
        keyboard.SetClearButtonImageSource("Controls/ImageSourceMathInputButtons/flame.png");
      }
    };

    public void UseMyCustomizedPlaceholderAppearance() {
      // You could also customize the "Active" placeholder nucleus and color, but for this example we don't.
      Atom.LaTeXSettings.PlaceholderRestingNucleus = "■";
      Atom.LaTeXSettings.PlaceholderRestingColor = CalculateMyPlaceholderRestingColorFromSurroundingTextColor(OutputMathPainter.TextColor.ToFormsColor());
    }
    public static Color CalculateMyPlaceholderRestingColorFromSurroundingTextColor(Color textColor) => textColor.WithLuminosity(textColor.Luminosity > 0.5 ? 0.2 : 0.8);
  }
}