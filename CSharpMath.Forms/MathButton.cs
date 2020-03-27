using Xamarin.Forms;
using SkiaSharp.Views.Forms;
namespace CSharpMath.Forms {
  using SkiaSharp;
  public class MathButton : ImageButton {
    public MathButton() {
      Aspect = Aspect.AspectFit;
      BackgroundColor = Color.Transparent;
    }
    private static void Paint(BindableObject o, object _, object __) {
      var b = (MathButton)o;
      b.Source = ImageSource.FromStream(() => new MathPainter {
        FontSize = b.Clarity,
        // Appropriate positioning for non-full characters, e.g. prime, degree
        // Also acts as spacing between MathButtons next to each other
        LaTeX = @"{\color{#0000}|}" + b.LaTeX + @"{\color{#0000}|}",
        TextColor = b.TextColor.ToSKColor()
      }.DrawAsStream());
    }
    static readonly MathPainter staticPainter = new MathPainter();
    public static readonly BindableProperty LaTeXProperty =
      BindableProperty.Create(nameof(LaTeX), typeof(string), typeof(MathButton), staticPainter.LaTeX, propertyChanged: Paint);
    public string LaTeX { get => (string)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    public static readonly BindableProperty TextColorProperty =
      BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MathButton), staticPainter.TextColor.ToFormsColor(), propertyChanged: Paint);
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly BindableProperty ClarityProperty =
      BindableProperty.Create(nameof(Clarity), typeof(float), typeof(MathButton), 50f, propertyChanged: Paint);
    public float Clarity { get => (float)GetValue(ClarityProperty); set => SetValue(ClarityProperty, value); }
  }
}