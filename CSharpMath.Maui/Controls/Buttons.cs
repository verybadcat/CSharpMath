using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace CSharpMath.Maui {
  using System;
  using System.Threading;

  public interface IButtonDraw { void ButtonDraw(); }
  [ContentProperty(nameof(Content))]
  public abstract class BaseButton<TView, TPainter, TContent> : ImageButton, IButtonDraw
    where TView : BaseView<TPainter, TContent>
    where TPainter : Rendering.FrontEnd.Painter<(ICanvas, SizeF), TContent, Color>, new()
    where TContent : class {
    public BaseButton() {
      Aspect = DefaultButtonStyle.AspectFit;
      BackgroundColor = DefaultButtonStyle.TransparentBackground;
      Content.PropertyChanged += (s, e) => ButtonDraw();
    }
    public virtual void ButtonDraw() => Source = ImageSource.FromStream(async _ => {
      if (Content is { } c) {
        var painter = c.Painter.ShallowClone();

        if (painter.FontSize is Rendering.FrontEnd.PainterConstants.DefaultFontSize)
          painter.FontSize = Rendering.FrontEnd.PainterConstants.LargerFontSize;

        // Appropriate positioning for non-full characters, e.g. prime, degree
        // Also acts as spacing between MathButtons next to each other
        // TODO: Implement and use \phantom
        painter.LaTeX = LatexHelper.phantom + painter.LaTeX + LatexHelper.phantom;

        var stream = new System.IO.MemoryStream();
        await painter.DrawToStreamAsync(stream);
        stream.Position = 0;
        return stream;
      }
      return null;
    });
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
  internal static readonly Color TransparentBackground = Colors.Transparent;
}