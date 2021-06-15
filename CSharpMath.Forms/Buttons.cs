using SkiaSharp;
using Xamarin.Forms;
namespace CSharpMath.Forms {
  using System;
  using SkiaSharp;
  public interface IButtonDraw { void ButtonDraw(); }
  [ContentProperty(nameof(Content))]
  public abstract class BaseButton<TView, TPainter, TContent> : ImageButton, IButtonDraw
    where TView : BaseView<TPainter, TContent>
    where TPainter : Rendering.FrontEnd.Painter<SKCanvas, TContent, SKColor>, new()
    where TContent : class {
    private readonly object lockObj = new object();
    public BaseButton() {
      Aspect = DefaultButtonStyle.AspectFit;
      BackgroundColor = DefaultButtonStyle.TransparentBackground;
      ButtonDraw();
    }
    public virtual void ButtonDraw() => Source = ImageSource.FromStream(() => {
      if (Content is { } c) {
        lock (lockObj) {
          var painter = c.Painter;
          var originalLatexString = painter.LaTeX;

          if (painter.FontSize is Rendering.FrontEnd.PainterConstants.DefaultFontSize)
            painter.FontSize = Rendering.FrontEnd.PainterConstants.LargerFontSize;

          if (TextColor != Color.Black && painter.LaTeX != null)
            painter.LaTeX = LatexHelper.SetColor(painter.LaTeX, TextColor);

          // Appropriate positioning for non-full characters, e.g. prime, degree
          // Also acts as spacing between MathButtons next to each other
          // TODO: Implement and use \phantom
          painter.LaTeX = LatexHelper.phantom + painter.LaTeX + LatexHelper.phantom;

          var stream = painter.DrawAsStream();
          painter.LaTeX = originalLatexString;
          return stream;
        }
      }
      return null;
    });
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly BindableProperty TextColorProperty = BindablePropertyWithButtonDraw<BaseButton<TView, TPainter, TContent>>(nameof(TextColor), typeof(Color), Color.Black);
    public TView? Content { get => (TView?)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(TView), typeof(BaseButton<TView, TPainter, TContent>));
    protected static BindableProperty BindablePropertyWithButtonDraw<TButton>(string propertyName, Type propertyType, object? defaultValue = null) where TButton : IButtonDraw =>
      BindableProperty.Create(propertyName, propertyType, typeof(TButton), defaultValue: defaultValue, propertyChanged: (b, o, n) => ((IButtonDraw)b).ButtonDraw());
  }
  public class MathButton : BaseButton<MathView, MathPainter, Atom.MathList> { }
  public class TextButton : BaseButton<TextView, TextPainter, Rendering.Text.TextAtom> { }
}
internal class DefaultButtonStyle {
  internal const Aspect AspectFit = Aspect.AspectFit;
  internal static readonly Color TransparentBackground = Color.Transparent;
}