namespace CSharpMath.Maui {
  using System;
  using CSharpMath.Atom;
  using Editor;
  using Microsoft.Maui.Controls;
  using Microsoft.Maui.Graphics;
  using Rendering.FrontEnd;

  public class MathInputButton : TextButton {
    readonly static Color Null = Colors.Transparent;
    Color? GetNullableColor(BindableProperty bindableProperty) {
      var v = (Color)GetValue(bindableProperty);
      return v == Null ? (Color?)null : v;
    }
    void SetNullableColor(BindableProperty bindableProperty, Color? value) => SetValue(bindableProperty, value ?? Null);

    static string ReplaceFirstOccurrence(string source, string subString, string replacement) => ReplaceOccurrence(source, subString, replacement, source.IndexOf(subString));
    static string ReplaceLastOccurrence(string source, string subString, string replacement) => ReplaceOccurrence(source, subString, replacement, source.LastIndexOf(subString));
    static string ReplaceOccurrence(string source, string subString, string replacement, int index) => index == -1 ? source : source.Remove(index, subString.Length).Insert(index, replacement);
    static int SubStringCount(string text, string substring) =>
      text.Split(new[] { substring }, StringSplitOptions.None).Length - 1;

    public MathInputButton() => Command = new Command(() => Keyboard?.KeyPress(Input));
    public MathKeyboard? Keyboard { get => (MathKeyboard?)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(MathKeyboard), typeof(MathInputButton));
    public MathKeyboardInput Input { get => (MathKeyboardInput)GetValue(InputProperty); set => SetValue(InputProperty, value); }
    public static readonly BindableProperty InputProperty = BindablePropertyWithButtonDraw<MathInputButton>(nameof(Input), typeof(MathKeyboardInput));
    public Color? PlaceholderActiveColor {
      get => GetNullableColor(PlaceholderActiveColorProperty);
      set => SetNullableColor(PlaceholderActiveColorProperty, value);
    }
    public static readonly BindableProperty PlaceholderActiveColorProperty = BindablePropertyWithButtonDraw<MathInputButton>(nameof(PlaceholderActiveColor), typeof(Color), defaultValue: Null);
    public Color? PlaceholderRestingColor {
      get => GetNullableColor(PlaceholderRestingColorProperty);
      set => SetNullableColor(PlaceholderRestingColorProperty, value);
    }
    public static readonly BindableProperty PlaceholderRestingColorProperty = BindablePropertyWithButtonDraw<MathInputButton>(nameof(PlaceholderRestingColor), typeof(Color), defaultValue: Null);
    public override void ButtonDraw() {
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
          Color? restingPlaceholderColor = PlaceholderRestingColor ?? LaTeXSettings.PlaceholderRestingColor?.ToMauiColor();
          Color? activePlaceholderColor = PlaceholderActiveColor ?? LaTeXSettings.PlaceholderActiveColor?.ToMauiColor();
          if (restingPlaceholderColor != null || activePlaceholderColor != null) {
            var restingNucleus = LaTeXSettings.PlaceholderRestingNucleus.Replace("\u25A1", @"\square");
            var coloredPlaceholderRestingNucleus = LatexHelper.SetColor(restingNucleus, restingPlaceholderColor);
            var coloredPlaceholderActiveNucleus = LatexHelper.SetColor(LaTeXSettings.PlaceholderActiveNucleus, activePlaceholderColor);
            if (Input == MathKeyboardInput.Power || Input == MathKeyboardInput.Subscript) {
              latex = ReplaceFirstOccurrence(latex, restingNucleus, coloredPlaceholderRestingNucleus);
              latex = ReplaceLastOccurrence(latex, LaTeXSettings.PlaceholderActiveNucleus, coloredPlaceholderActiveNucleus);
            } else if (LaTeXSettings.PlaceholderRestingNucleus == LaTeXSettings.PlaceholderActiveNucleus && SubStringCount(latex, LaTeXSettings.PlaceholderActiveNucleus) > 1
                || restingNucleus != LaTeXSettings.PlaceholderActiveNucleus && latex.Contains(restingNucleus) && latex.Contains(LaTeXSettings.PlaceholderActiveNucleus)) {
              latex = ReplaceFirstOccurrence(latex, LaTeXSettings.PlaceholderActiveNucleus, coloredPlaceholderActiveNucleus);
              latex = ReplaceLastOccurrence(latex, restingNucleus, coloredPlaceholderRestingNucleus);
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