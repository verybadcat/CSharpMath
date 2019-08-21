using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  using Rendering;
  using static SkiaSharp.SkiaColorExtensions;
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class MathKeyboard : ContentView {
    public enum Tab {
      Numbers = 1, Operations, Functions, Letters, LettersCapitals
    }
    public MathKeyboard() {
      InitializeComponent();
      CurrentTab = Tab.Numbers;
    }

    public void BindDisplay(SKCanvasView view, SkiaSharp.MathPainter settings, SKColor caretColor, CaretShape caretShape = CaretShape.IBeam, SKStrokeCap cap = SKStrokeCap.Butt) {
      view.EnableTouchEvents = true;
      view.Touch +=
        (sender, e) => {
          if (e.ActionType == SKTouchAction.Pressed)
            keyboard.Tap(new System.Drawing.PointF(e.Location.X, e.Location.Y));
        };
      keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => {
          var c = e.Surface.Canvas;
          c.Clear();
          SkiaSharp.MathPainter.DrawDisplay(settings, keyboard.Display, c);
          keyboard.DrawCaret(new SkiaSharp.SkiaCanvas(c, cap, false), caretColor.FromNative(), caretShape);
        };
    }

    public Rendering.MathKeyboard UnderlyingKeyboard => keyboard;
    public void KeyPress(Editor.MathKeyboardInput input) => keyboard.KeyPress(input);
    public void Tap(System.Drawing.PointF point) => keyboard.Tap(point);

    public event EventHandler RedrawRequested {
      add => keyboard.RedrawRequested += value;
      remove => keyboard.RedrawRequested -= value;
    }
    public event EventHandler ReturnPressed {
      add => keyboard.ReturnPressed += value;
      remove => keyboard.ReturnPressed -= value;
    }
    public event EventHandler DismissPressed {
      add => keyboard.DismissPressed += value;
      remove => keyboard.DismissPressed -= value;
    }

    public double SelectedBorderWidth { get; set; } = 3;
    public Color SelectedBorderColor { get; set; } = Color.Orange;
    public IDisplay<Fonts, Glyph> Display => keyboard.Display;
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


    private void NumbersButton_Clicked(object sender, EventArgs e) => CurrentTab = Tab.Numbers;
    private void OperationsButton_Clicked(object sender, EventArgs e) => CurrentTab = Tab.Operations;
    private void FunctionsButton_Clicked(object sender, EventArgs e) => CurrentTab = Tab.Functions;
    private void LettersButton_Clicked(object sender, EventArgs e) => CurrentTab = Tab.Letters;
    private void Shift_Clicked(object sender, EventArgs e) => CurrentTab = Tab.LettersCapitals;
    private void ShiftCapitals_Clicked(object sender, EventArgs e) => CurrentTab = Tab.Letters;
  }
}