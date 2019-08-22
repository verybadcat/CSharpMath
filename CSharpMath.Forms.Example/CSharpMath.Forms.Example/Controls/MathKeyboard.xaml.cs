using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms.Example {
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {
    public enum Tab {
      Numbers = 1, Operations, Functions, Letters, LettersCapitals
    }
    public MathKeyboard() {
      InitializeComponent();
      CurrentTab = Tab.Numbers;
    }

    public Rendering.MathKeyboard ViewModel => keyboard;

    public double SelectedBorderWidth { get; set; } = 3;
    public Color SelectedBorderColor { get; set; } = Color.Orange;
    private Tab _tab;
    public Tab CurrentTab {
      get => _tab;
      set {
        Numbers.IsVisible = false;
        Operations.IsVisible = false;
        Functions.IsVisible = false;
        Letters.IsVisible = false;
        LettersCapitals.IsVisible = false;
        NumbersButton.BorderWidth = 0;
        OperationsButton.BorderWidth = 0;
        FunctionsButton.BorderWidth = 0;
        LettersButton.BorderWidth = 0;

        void SetState(Grid grid, Button button) {
            grid.IsVisible = true;
            button.BorderWidth = SelectedBorderWidth;
            button.BorderColor = SelectedBorderColor;
        }
        switch (value) {
          case Tab.Numbers:
            SetState(Numbers, NumbersButton);
            break;
          case Tab.Operations:
            SetState(Operations, OperationsButton);
            break;
          case Tab.Functions:
            SetState(Functions, FunctionsButton);
            break;
          case Tab.Letters:
            SetState(Letters, LettersButton);
            break;
          case Tab.LettersCapitals:
            SetState(LettersCapitals, LettersButton);
            break;
        }
        ShiftCapitalsButton.BorderWidth = SelectedBorderWidth;
        ShiftCapitalsButton.BorderColor = SelectedBorderColor;
        _tab = value;
      }
    }
  }
}