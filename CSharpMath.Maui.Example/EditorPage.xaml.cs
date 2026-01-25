using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
#if !IOS && !ANDROID
using SharpHook;
using SharpHook.Data;
#endif

namespace CSharpMath.Maui.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class EditorPage : ContentPage {
    public EditorPage() {
      InitializeComponent();
      Content = new EditorView();
    }
  }
  public class EditorView : ContentView {
    public MathPainter OutputMathPainter = new MathPainter { TextColor = Colors.Black };
    public GraphicsView OutputGraphicsView;
    readonly MathKeyboard keyboard = new(Rendering.FrontEnd.PainterConstants.LargerFontSize);
    public EditorView() {
      // Basic functionality
      OutputGraphicsView = new GraphicsView();
      var viewModel = keyboard.Keyboard;
      viewModel.BindDisplay(OutputGraphicsView, OutputMathPainter, new Color(0, 0, 0, 153));

      // Input from physical keyboard (doesn't work on mobile because SharpHook depends on libuiohook which only works on desktop)
#if !IOS && !ANDROID
      var popupShown = false;
      var hook = new SimpleGlobalHook(GlobalHookType.Keyboard);
      void activatedListener(object? sender, EventArgs e) { if (!hook.IsRunning && IsLoaded && !popupShown) hook.RunAsync(); }
      void deactivatedListener(object? sender, EventArgs e) { if (hook.IsRunning) hook.Stop(); }
      Window? owningWindow = null;
      void unloadedListener(object? sender, EventArgs e) {
        if (hook.IsRunning) hook.Stop();
        owningWindow?.Activated -= activatedListener;
        owningWindow?.Deactivated -= deactivatedListener;
        Unloaded -= unloadedListener;
      }
      Loaded += (sender, e) => {
        owningWindow = Window;
        if (!hook.IsRunning && owningWindow.IsActivated && !popupShown) hook.RunAsync();
        owningWindow.Activated += activatedListener;
        owningWindow.Deactivated += deactivatedListener;
        Unloaded += unloadedListener;
      };
      hook.KeyTyped += (sender, e) => viewModel.KeyPress((Editor.MathKeyboardInput)e.Data.KeyChar);
      hook.KeyPressed += (sender, e) => {
        switch (e.Data.KeyCode) {
          case KeyCode.VcBackspace: viewModel.KeyPress(Editor.MathKeyboardInput.Backspace); break;
          case KeyCode.VcNumPadEnter or KeyCode.VcEnter: viewModel.KeyPress(Editor.MathKeyboardInput.Return); break;
          case KeyCode.VcLeft: viewModel.KeyPress(Editor.MathKeyboardInput.Left); break;
          case KeyCode.VcRight: viewModel.KeyPress(Editor.MathKeyboardInput.Right); break;
          case KeyCode.VcUp: viewModel.KeyPress(Editor.MathKeyboardInput.Up); break;
          case KeyCode.VcDown: viewModel.KeyPress(Editor.MathKeyboardInput.Down); break;
        }
      };
#endif

      // Evaluation
      keyboard.Keyboard.ReturnPressed += delegate {
        Dispatcher.Dispatch(async () => { // We may not be on the main thread since SharpHook event handlers are called from its own threads.
          if (Parent is Page p) {
#if !IOS && !ANDROID
            popupShown = true;
            if (hook.IsRunning) hook.Stop();
#endif
            var view = new MathView { FontSize = 32, EnablePanning = true, TextAlignment = Rendering.FrontEnd.TextAlignment.TopLeft,
              LaTeX = Evaluation.Interpret(keyboard.Keyboard.MathList)
            };
            await p.ShowPopupAsync(new VerticalStackLayout {
              new Grid {
                ColumnDefinitions = { new() { Width = new GridLength(1, GridUnitType.Star) }, new() { Width = new GridLength(1, GridUnitType.Star) } },
                Children = {
                  GridItem(0, 0, new Button { Text = "Reset pan", Command = new Command(() => view.DisplacementX = view.DisplacementY = 0) }),
                  GridItem(0, 1, new Button { Text = "Close", Command = new Command(() => p.ClosePopupAsync()) }) }
              }, view });
#if !IOS && !ANDROID
            popupShown = false;
            if (IsLoaded && owningWindow is { IsActivated: true } && !hook.IsRunning) await hook.RunAsync();
#endif
          }
        });
      };

      // Debug labels
      var latex = new Label { Text = "LaTeX = " };
      var atomTypes = new Label { Text = "Atom Types = " };
      var ranges = new Label { Text = "Ranges = " };
      var index = new Label { Text = "Index = " };
      viewModel.RedrawRequested += (sender, e) => Dispatcher.Dispatch(() => {
        latex.Text = "LaTeX = " + viewModel.LaTeX;
        atomTypes.Text = "Atom Types = " + string.Join
          (", ", viewModel.MathList.Select(x => x.GetType().Name));
        ranges.Text = "Ranges = " + string.Join
          (", ", (viewModel.Display ?? throw new Atom.InvalidCodePathException("Invalid LaTeX"))
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
          new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
          new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
          new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
          new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
          new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
        },
        Children = {
          GridItem(0, 0, new ScrollView {
            Content = new StackLayout { latex, atomTypes, ranges, index }
          }),
          GridItem(1, 0, OutputGraphicsView),
          GridItem(2, 0, new BoxView { Color = Colors.Gray }),
          GridItem(3, 0, keyboard),
          GridItem(4, 0, new StackLayout { new Button { Text = "Change appearance", Command = new Command(ChangeAppearance), HorizontalOptions = LayoutOptions.Start } }),
        }
      };
      Themes[0].Invoke();
    }
    int CurrentThemeIndex = 0;
    public void ChangeAppearance() {
      CurrentThemeIndex = (CurrentThemeIndex + 1) % Themes.Count;
      Themes[CurrentThemeIndex].Invoke();
      keyboard.Keyboard.InsertionIndex = keyboard.Keyboard.InsertionIndex; // Hack to redraw placeholders in the output.
    }
    IList<Action> Themes => [
      () => { // This theme is the default.
        var color = (Color)Application.Current!.Resources[Application.Current!.RequestedTheme == AppTheme.Dark ? "PrimaryDark" : "Primary"];
        OutputMathPainter.TextColor = color;
        keyboard.Keyboard.UpdateDisplayCaret(OutputGraphicsView, OutputMathPainter, color);
        Atom.LaTeXSettings.PlaceholderBlinks = false;
        Atom.LaTeXSettings.PlaceholderActiveColor = null;
        Atom.LaTeXSettings.PlaceholderRestingColor = null;
        Atom.LaTeXSettings.PlaceholderActiveNucleus = "■";
        Atom.LaTeXSettings.PlaceholderRestingNucleus = "□";
        keyboard.SetButtonsTextColor(color);
        keyboard.SetButtonsTextColor(color);
        keyboard.SetClearButtonImageSource("recyclebin.png");
      },
      () => {
        var color = (Color)Application.Current!.Resources[Application.Current!.RequestedTheme == AppTheme.Dark ? "PrimaryDark" : "Primary"];
        UseMyCustomizedPlaceholderAppearance();
        keyboard.Keyboard.UpdateDisplayCaret(OutputGraphicsView, OutputMathPainter, color);
        keyboard.SetButtonsTextColor(color); // Placeholder appearance on the keys is the same as in the output by default.
        keyboard.SetClearButtonImageSource("metaltrashcan.png");
      },
      () => {
        Atom.LaTeXSettings.PlaceholderBlinks = true;
        OutputMathPainter.TextColor = Application.Current!.RequestedTheme == AppTheme.Dark ? Colors.LightGreen : Colors.DarkGreen;
        UseMyCustomizedPlaceholderAppearance();
        keyboard.Keyboard.UpdateDisplayCaret(OutputGraphicsView, OutputMathPainter, Colors.Orange);
        // If you'd like to use different keyboard colors than output colors and you specified a placeholder color,
        // probably you'll not want to use the same placeholder color on the keyboard:
        keyboard.SetButtonsTextColor(Colors.Brown, CalculateMyPlaceholderRestingColorFromSurroundingTextColor(Colors.Brown));
        keyboard.SetClearButtonImageSource("flame.png");
      }
    ];

    public void UseMyCustomizedPlaceholderAppearance() {
      // You could also customize the "Active" placeholder nucleus and color, but for this example we don't.
      Atom.LaTeXSettings.PlaceholderRestingNucleus = "■";
      Atom.LaTeXSettings.PlaceholderRestingColor = CalculateMyPlaceholderRestingColorFromSurroundingTextColor(OutputMathPainter.TextColor).ToCSharpMathColor();
    }
    public static Color CalculateMyPlaceholderRestingColorFromSurroundingTextColor(Color textColor) => textColor.WithLuminosity(textColor.GetLuminosity() > 0.5f ? 0.2f : 0.8f);
  }
}