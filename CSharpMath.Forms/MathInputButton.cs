using Xamarin.Forms;
namespace CSharpMath.Forms {
  using Editor;
  using Rendering.FrontEnd;
  public class MathInputButton : MathButton {
    public MathInputButton() => Command = new Command(() => Keyboard?.KeyPress(Input));
    public static readonly BindableProperty KeyboardProperty =
      BindableProperty.Create(nameof(Keyboard), typeof(MathKeyboard), typeof(MathInputButton));
    public MathKeyboard Keyboard { get => (MathKeyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }
    static string InputToLaTeX(MathKeyboardInput input) {
      switch (input) {
        case MathKeyboardInput.Left: return "\u25C0";
        case MathKeyboardInput.Right: return "\u25B6";
        case MathKeyboardInput.Up: return "\u25B2";
        case MathKeyboardInput.Down: return "\u25BC";
        case MathKeyboardInput.Backspace: return "{\\color{red}\u2B05}";
        case MathKeyboardInput.Return: return "\u21B5";
        case MathKeyboardInput.Clear: return "{\\color{red}\u21BB}";
        case MathKeyboardInput.Dismiss: return "\u2A2F";
        case MathKeyboardInput.Space: return @"\ ␣\ ";
        default:
          var keyboard = new MathKeyboard();
          keyboard.KeyPress(input);
          return keyboard.LaTeX;
      }
    }
    public static readonly BindableProperty InputProperty =
      BindableProperty.Create(nameof(Input), typeof(MathKeyboardInput), typeof(MathInputButton), propertyChanged:(b, o, n) => {
        var button = (MathInputButton)b;
        button.Content ??= new MathView();
        button.Content.LaTeX = InputToLaTeX((MathKeyboardInput)n);
      });
    public MathKeyboardInput Input { get => (MathKeyboardInput)GetValue(InputProperty); set => SetValue(InputProperty, value); }
  }
}
