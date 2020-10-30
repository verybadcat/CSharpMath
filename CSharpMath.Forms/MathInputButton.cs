using Xamarin.Forms;
namespace CSharpMath.Forms {
  using System;
  using System.Text.RegularExpressions;
  using CSharpMath.Atom;
  using Editor;
  using Rendering.FrontEnd;
  public class MathInputButton : TextButton {
    public MathInputButton() => Command = new Command(() => Keyboard?.KeyPress(Input));
    public MathKeyboard? Keyboard { get => (MathKeyboard?)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(MathKeyboard), typeof(MathInputButton));
    public MathKeyboardInput Input { get => (MathKeyboardInput)GetValue(InputProperty); set => SetValue(InputProperty, value); }
    public static readonly BindableProperty InputProperty = BindablePropertyWithButtonDraw(nameof(Input), typeof(MathKeyboardInput));
    public Color? PlaceholderActiveColor {
      get => (Color?)GetValue(PlaceholderActiveColorProperty);
      set => SetValue(PlaceholderActiveColorProperty, value);
    }
    public static readonly BindableProperty PlaceholderActiveColorProperty = BindablePropertyWithButtonDraw(nameof(PlaceholderActiveColor), typeof(Color?), defaultValue: null);
    public Color? PlaceholderRestingColor {
      get => (Color?)GetValue(PlaceholderRestingColorProperty);
      set => SetValue(PlaceholderRestingColorProperty, value);
    }
    public static readonly BindableProperty PlaceholderRestingColorProperty = BindablePropertyWithButtonDraw(nameof(PlaceholderRestingColor), typeof(Color?), defaultValue: null);
    static BindableProperty BindablePropertyWithButtonDraw(string propertyName, Type propertyType, object? defaultValue = null) =>
      BindableProperty.Create(propertyName, propertyType, typeof(MathInputButton), defaultValue: defaultValue, propertyChanged: (b, o, n) => ((MathInputButton)b).ButtonDraw());
    protected override void ButtonDraw() {
      Content ??= new TextView();
      switch (Input) {
        case MathKeyboardInput.Left: Content.LaTeX = "\u25C0"; break;
        case MathKeyboardInput.Right: Content.LaTeX = "\u25B6"; break;
        case MathKeyboardInput.Up: Content.LaTeX = "\u25B2"; break;
        case MathKeyboardInput.Down: Content.LaTeX = "\u25BC"; break;
        case MathKeyboardInput.Backspace: Content.LaTeX = "\u2B05"; break;
        case MathKeyboardInput.Return: Content.LaTeX = "\u21B5"; break;
        case MathKeyboardInput.Clear: Content.LaTeX = "\u21BB"; break;
        case MathKeyboardInput.Dismiss: Content.LaTeX = "\u2A2F"; break;
        case MathKeyboardInput.Space: Content.LaTeX = @"\ ␣\ "; break;
        default:
          var keyboard = new MathKeyboard();
          keyboard.KeyPress(Input);
          var latex = keyboard.LaTeX;
          Color? restingPlaceholderColor = PlaceholderRestingColor ?? LaTeXSettings.PlaceholderRestingColor;
          Color? activePlaceholderColor = PlaceholderActiveColor ?? LaTeXSettings.PlaceholderActiveColor;
          if (restingPlaceholderColor != null || activePlaceholderColor != null) {
            var restingNucleus = LaTeXSettings.PlaceholderRestingNucleus.Replace("\u25A1", @"\square");
            var coloredPlaceholderRestingNucleus = LatexHelper.SetColor(restingNucleus, restingPlaceholderColor);
            var coloredPlaceholderActiveNucleus = LatexHelper.SetColor(LaTeXSettings.PlaceholderActiveNucleus, activePlaceholderColor);
            if (Input == MathKeyboardInput.Power || Input == MathKeyboardInput.Subscript) {
              latex = latex.ReplaceFirstOccurrence(restingNucleus, coloredPlaceholderRestingNucleus);
              latex = latex.ReplaceLastOccurrence(LaTeXSettings.PlaceholderActiveNucleus, coloredPlaceholderActiveNucleus);
            } else if (LaTeXSettings.PlaceholderRestingNucleus == LaTeXSettings.PlaceholderActiveNucleus && Regex.Matches(latex, LaTeXSettings.PlaceholderActiveNucleus).Count > 1
                || restingNucleus != LaTeXSettings.PlaceholderActiveNucleus && latex.Contains(restingNucleus) && latex.Contains(LaTeXSettings.PlaceholderActiveNucleus)) {
              latex = latex.ReplaceFirstOccurrence(LaTeXSettings.PlaceholderActiveNucleus, coloredPlaceholderActiveNucleus);
              latex = latex.ReplaceLastOccurrence(restingNucleus, coloredPlaceholderRestingNucleus);
            } else {
              latex = latex.Replace(LaTeXSettings.PlaceholderActiveNucleus, coloredPlaceholderActiveNucleus);
            }
          }
          Content.LaTeX = @$"\({latex}\)";
          break;
      }
      base.ButtonDraw();
    }
  }
  public class ImageSourceMathInputButton : ImageButton {
    public ImageSourceMathInputButton() {
      Aspect = DefaultButtonStyle.AspectFit;
      BackgroundColor = DefaultButtonStyle.TransparentBackground;
      Command = new Command(() => Keyboard?.KeyPress(Input));
    }
    public MathKeyboard? Keyboard { get => (MathKeyboard?)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(MathKeyboard), typeof(ImageSourceMathInputButton));
    public MathKeyboardInput Input { get => (MathKeyboardInput)GetValue(InputProperty); set => SetValue(InputProperty, value); }
    public static readonly BindableProperty InputProperty = BindableProperty.Create(nameof(Input), typeof(MathKeyboardInput), typeof(ImageSourceMathInputButton));
  }
}