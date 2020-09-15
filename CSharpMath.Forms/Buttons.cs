using SkiaSharp;
using Xamarin.Forms;
namespace CSharpMath.Forms {
  using SkiaSharp;
  [ContentProperty(nameof(Content))]
  public abstract class BaseButton<TView, TPainter, TContent> : ImageButton
    where TView : BaseView<TPainter, TContent>
    where TPainter : Rendering.FrontEnd.Painter<SKCanvas, TContent, SKColor>, new()
    where TContent : class {
    private Color _textColor = Color.Black;
    public Color TextColor {
      get => _textColor;
      set {
        _textColor = value;
        SetImageSource();
      }
    }
    public BaseButton() {
      Aspect = Aspect.AspectFit;
      BackgroundColor = Color.Transparent;
      SetImageSource();
    }

    private void SetImageSource() => Source = ImageSource.FromStream(() => {
      if (Content is { } c) {
        var painter = c.Painter;
        var originalLatexString = painter.LaTeX;
        HaveLargerFontSizeByDefault(painter);
        SetTextColorIfRequested(painter);
        FixPositionOfNonFullCharacters(painter);
        var stream = painter.DrawAsStream();
        painter.LaTeX = originalLatexString;
        return stream;
      }
      return null;
    });

    private static void HaveLargerFontSizeByDefault(TPainter painter) {
      if (painter.FontSize is Rendering.FrontEnd.PainterConstants.DefaultFontSize)
        painter.FontSize = Rendering.FrontEnd.PainterConstants.LargerFontSize;
    }
    private void SetTextColorIfRequested(TPainter painter) {
      if (TextColor != Color.Black && painter.LaTeX != null)
        painter.LaTeX = SetColor(painter.LaTeX, TextColor);
    }
    private void FixPositionOfNonFullCharacters(TPainter painter) {
      // Appropriate positioning for non-full characters, e.g. prime, degree
      // Also acts as spacing between MathButtons next to each other
      // TODO: Implement and use \phantom
      string phantom = SetColor("|", Color.Transparent);
      painter.LaTeX = phantom + painter.LaTeX + phantom;
    }
    static string SetColor(string latex, Color color) => @"{\color{" + color.ToHex() + "}{" + latex + "}}";

    public static readonly BindableProperty TextColorProperty =
      BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(BaseButton<TView, TPainter, TContent>), propertyChanged: (b, o, n) => {
        var baseButton = (BaseButton<TView, TPainter, TContent>)b;
        baseButton.TextColor = (Color)n;
      });
    public TView? Content { get => (TView?)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(TView), typeof(BaseButton<TView, TPainter, TContent>));
  }
  public class MathButton : BaseButton<MathView, MathPainter, Atom.MathList> { }
  public class TextButton : BaseButton<TextView, TextPainter, Rendering.Text.TextAtom> { }
}