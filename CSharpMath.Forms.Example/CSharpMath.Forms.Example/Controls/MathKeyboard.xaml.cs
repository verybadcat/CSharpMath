using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {
    public enum Tab {
      Numbers = 1, Symbols, Functions, Operations, Letters, LettersCapitals
    }
    public MathKeyboard() {
      InitializeComponent();
      CurrentTab = Tab.Numbers;
    }
    public Rendering.FrontEnd.MathKeyboard ViewModel => keyboard;
    public double SelectedBorderWidth { get; set; } = 3;
    public Color SelectedBorderColor { get; set; } = Color.Orange;
    private Tab _tab;
    public Tab CurrentTab {
      get => _tab;
      set {
        foreach (var grid in new[] {
          Numbers, Symbols, Functions, Operations, Letters, LettersCapitals
        }) grid.IsVisible = false;
        foreach (var gridButton in new[] {
          NumbersButton, SymbolsButton, FunctionsButton, OperationsButton, LettersButton
        }) { gridButton.BorderWidth = 0; gridButton.BorderColor = Color.Default; }
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
        selectedGridButton.BorderWidth = SelectedBorderWidth;
        selectedGridButton.BorderColor = SelectedBorderColor;
        ShiftCapitalsButton.BorderWidth = SelectedBorderWidth;
        ShiftCapitalsButton.BorderColor = SelectedBorderColor;
        _tab = value;
      }
    }
  }
}