using System;
using System.Linq;
using CSharpMath.Editor;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {
    public enum Tab {
      Numbers = 1, Symbols, Functions, Operations, Letters, LettersCapitals
    }
    public Rendering.FrontEnd.MathKeyboard Keyboard { get; }
    public MathKeyboard(float fontSize = Rendering.FrontEnd.PainterConstants.DefaultFontSize) {
      Keyboard = new Rendering.FrontEnd.MathKeyboard(fontSize);
      Resources.Add(nameof(Keyboard), Keyboard);
      InitializeComponent();
      CurrentTab = Tab.Numbers;
    }
    public Color SelectedBackgroundColor { get; set; } = Color.Orange;
    private Tab _tab;
    public Tab CurrentTab {
      get => _tab;
      set {
        foreach (var buttonGrid in ButtonGrids) buttonGrid.IsVisible = false;
        foreach (var tabButton in TabButtons) tabButton.BackgroundColor = Color.Transparent;
        var (selectedGrid, selectedGridButton) =
          value switch {
            Tab.Numbers => (Numbers, NumbersButton),
            Tab.Symbols => (Symbols, SymbolsButton),
            Tab.Functions => (Functions, FunctionsButton),
            Tab.Operations => (Operations, OperationsButton),
            Tab.Letters => (Letters, LettersButton),
            Tab.LettersCapitals => (LettersCapitals, LettersButton),
            _ => throw new System.ComponentModel.InvalidEnumArgumentException
              (nameof(value), (int)value, typeof(Tab))
          };
        selectedGrid.IsVisible = true;
        selectedGridButton.BackgroundColor = SelectedBackgroundColor;
        ShiftCapitalsButton.BackgroundColor = SelectedBackgroundColor;
        _tab = value;
      }
    }
    public void SetButtonsTextColor(Color color, Color? placeholderRestingColor = null, Color? placeholderActiveColor = null) {
      LeftButton.TextColor = color;
      RightButton.TextColor = color;
      TabButtons.ForEach(button => button.TextColor = color);
      ButtonGrids.SelectMany(grid => grid.Children)
        .Where(child => child is MathInputButton button && button.Input != MathKeyboardInput.Backspace).Cast<MathInputButton>()
        .ForEach(button => {
          button.TextColor = color;
          button.PlaceholderRestingColor = placeholderRestingColor;
          button.PlaceholderActiveColor = placeholderActiveColor;
        });
    }
    
    public void SetClearButtonImageSource(ImageSource imageSource) => ButtonGrids.SelectMany(grid => grid.Children)
      .Where(button => button is ImageSourceMathInputButton).Cast<ImageSourceMathInputButton>()
      .Where(button => button.Input == MathKeyboardInput.Clear)
      .ForEach(button => button.Source = imageSource);
    MathButton[] TabButtons => new[] { NumbersButton, SymbolsButton, FunctionsButton, OperationsButton, LettersButton };
    Grid[] ButtonGrids => new[] { Numbers, Symbols, Functions, Operations, Letters, LettersCapitals };
  }
  [AcceptEmptyServiceProvider]
  public class SwitchToTabExtension : IMarkupExtension<Command> {
    public MathKeyboard.Tab Target { get; set; }
    public MathKeyboard? Self { get; set; }
    public Command ProvideValue(IServiceProvider _) =>
      Target is 0 ? throw new ArgumentNullException(nameof(Target)) :
      Self is null ? throw new ArgumentNullException(nameof(Self)) :
      new Command(() => Self.CurrentTab = Target);
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
}