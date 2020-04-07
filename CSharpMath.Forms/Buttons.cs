using SkiaSharp;
using Xamarin.Forms;
namespace CSharpMath.Forms {
  using SkiaSharp;
  [ContentProperty(nameof(Content))]
  public abstract class BaseButton<TView, TPainter, TContent> : ImageButton
    where TView : BaseView<TPainter, TContent>
    where TPainter : Rendering.FrontEnd.Painter<SKCanvas, TContent, SKColor>, new()
    where TContent : class {
    public BaseButton() {
      Aspect = Aspect.AspectFit;
      // Color.Default will be ugly: https://github.com/verybadcat/CSharpMath/issues/111
      BackgroundColor = Color.Transparent;
      Source = ImageSource.FromStream(() => {
        if (Content is { } c) {
          var latex = c.Painter.LaTeX;
          // Appropriate positioning for non-full characters, e.g. prime, degree
          // Also acts as spacing between MathButtons next to each other
          c.Painter.LaTeX = @"{\color{#0000}|}" + latex + @"{\color{#0000}|}";
          var stream = c.Painter.DrawAsStream();
          c.Painter.LaTeX = latex;
          return stream;
        }
        return null;
      });
    }
    public TView? Content { get => (TView?)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(TView), typeof(BaseButton<TView, TPainter, TContent>));
  }
  public class MathButton : BaseButton<MathView, MathPainter, Atom.MathList> { }
  public class TextButton : BaseButton<TextView, TextPainter, Rendering.Text.TextAtom> { }
}