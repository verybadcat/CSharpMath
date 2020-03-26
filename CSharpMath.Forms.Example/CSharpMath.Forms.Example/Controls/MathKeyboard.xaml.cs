using System;
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
    public Color SelectedBackgroundColor { get; set; } = Color.Orange;
    private Tab _tab;
    public Tab CurrentTab {
      get => _tab;
      set {
        foreach (var grid in new[] {
          Numbers, Symbols, Functions, Operations, Letters, LettersCapitals
        }) grid.IsVisible = false;
        foreach (var gridButton in new[] {
          NumbersButton, SymbolsButton, FunctionsButton, OperationsButton, LettersButton
        }) gridButton.BackgroundColor = Color.Default;
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
  }
  [AcceptEmptyServiceProvider]
  public class SwitchToTabExtension : IMarkupExtension<Command> {
    public MathKeyboard.Tab Target { get; set; }
    public MathKeyboard Self { get; set; }
    public Command ProvideValue(IServiceProvider _) =>
      Target is 0 ? throw new ArgumentNullException(nameof(Target)) :
      Self is null ? throw new ArgumentNullException(nameof(Self)) :
      new Command(() => Self.CurrentTab = Target);
    object IMarkupExtension.ProvideValue(IServiceProvider _) => ProvideValue(_);
  }
}