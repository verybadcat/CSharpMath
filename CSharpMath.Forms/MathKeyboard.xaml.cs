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
    public MathKeyboard() {
      InitializeComponent();
    }

    public void BindDisplay(SKCanvasView view, SkiaSharp.MathPainter settings, SKColor caretColor, CaretShape caretShape = CaretShape.IBeam, SKStrokeCap cap = SKStrokeCap.Butt) {
      view.EnableTouchEvents = true;
      view.Touch +=
        (sender, e) => {
          if (e.ActionType == SKTouchAction.Pressed)
            Keyboard.Tap(new System.Drawing.PointF(e.Location.X, e.Location.Y));
        };
      Keyboard.RedrawRequested += (_, __) => view.InvalidateSurface();
      view.PaintSurface +=
        (sender, e) => {
          var c = e.Surface.Canvas;
          c.Clear();
          SkiaSharp.MathPainter.DrawDisplay(settings, Keyboard.Display, c);
          Keyboard.DrawCaret(new SkiaSharp.SkiaCanvas(c, cap, false), caretColor.FromNative(), caretShape);
        };
    }

    public void Tap(System.Drawing.PointF point) => Keyboard.Tap(point);

    public event EventHandler RedrawRequested {
      add => Keyboard.RedrawRequested += value;
      remove => Keyboard.RedrawRequested -= value;
    }
    public event EventHandler ReturnPressed {
      add => Keyboard.ReturnPressed += value;
      remove => Keyboard.ReturnPressed -= value;
    }
    public event EventHandler DismissPressed {
      add => Keyboard.DismissPressed += value;
      remove => Keyboard.DismissPressed -= value;
    }

    public IDisplay<Fonts, Glyph> Display => Keyboard.Display;

    private void SwitchTab(Grid tab) {
      tab.IsVisible = true;
      if (tab != Numbers) Numbers.IsVisible = false;
      if (tab != Operations) Operations.IsVisible = false;
      if (tab != Functions) Functions.IsVisible = false;
      if (tab != Letters) Letters.IsVisible = false;
      if (tab != Letters2) Letters2.IsVisible = false;
    }

    private void Shift_Clicked(object sender, EventArgs e) {
      Letters.IsVisible ^= true;
      Letters2.IsVisible ^= true;
    }

    private void NumbersButton_Clicked(object sender, EventArgs e) => SwitchTab(Numbers);
    private void OperationsButton_Clicked(object sender, EventArgs e) => SwitchTab(Operations);
    private void FunctionsButton_Clicked(object sender, EventArgs e) => SwitchTab(Functions);
    private void LettersButton_Clicked(object sender, EventArgs e) => SwitchTab(Letters);
  }
}