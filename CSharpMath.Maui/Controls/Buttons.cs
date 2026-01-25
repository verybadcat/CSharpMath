using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace CSharpMath.Maui {
  using System.ComponentModel;
  using System.Collections.Generic;

  [ContentProperty(nameof(Content))]
  public abstract class BaseButton<TView, TPainter, TContent> : ImageButton
    where TView : BaseView<TPainter, TContent>
    where TPainter : Rendering.FrontEnd.Painter<(ICanvas, SizeF), TContent, Color>, new()
    where TContent : class {
    public BaseButton() {
      Aspect = DefaultButtonStyle.AspectFit;
      BackgroundColor = DefaultButtonStyle.TransparentBackground;
      PropertyChanging += (s, e) => {
        if (e.PropertyName == nameof(Content)) Content?.PropertyChanged -= ContentPropertyChanged;
      };
      PropertyChanged += (s, e) => {
        if (e.PropertyName == nameof(Content)) Content?.PropertyChanged += ContentPropertyChanged;
      };
      UpdateImageSource();
    }
    void ContentPropertyChanged(object? sender, PropertyChangedEventArgs e) {
      if (string.IsNullOrEmpty(e.PropertyName) /*all properties changed*/ ||
          BaseView<TPainter, TContent>.WritablePainterPropertyNames.Contains(e.PropertyName))
        UpdateImageSource();
    }
    public virtual void UpdateImageSource()
#if IOS || ANDROID || MACCATALYST || WINDOWS
      => Source = ImageSource.FromStream(async _ => {
      if (Content is { } c) {
        var painter = c.Painter.ShallowClone();

        if (painter.FontSize is Rendering.FrontEnd.PainterConstants.DefaultFontSize)
          painter.FontSize = Rendering.FrontEnd.PainterConstants.LargerFontSize;

        // Appropriate positioning for non-full characters, e.g. prime, degree
        // Also acts as spacing between MathButtons next to each other
        // TODO: Implement and use \phantom
        painter.LaTeX = LatexHelper.phantom + painter.LaTeX + LatexHelper.phantom;
        if (TextColor is { } color) painter.TextColor = color;

        var stream = new System.IO.MemoryStream();
        await painter.DrawToStreamAsync(stream);
        stream.Position = 0;
        return stream;
      }
      return null;
    });
#else
      { }
#endif
    public Color? TextColor { get => (Color?)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly BindableProperty TextColorProperty = BindablePropertyWithUpdateImageSource<BaseButton<TView, TPainter, TContent>>(nameof(TextColor), typeof(Color), null);
    public TView? Content { get => (TView?)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(TView), typeof(BaseButton<TView, TPainter, TContent>));
    protected static BindableProperty BindablePropertyWithUpdateImageSource<TButton>(string propertyName, System.Type propertyType, object? defaultValue = null) where TButton : BaseButton<TView, TPainter, TContent> =>
      BindableProperty.Create(propertyName, propertyType, typeof(TButton), defaultValue: defaultValue, propertyChanged: (b, o, n) => ((TButton)b).UpdateImageSource());
  }
  public class MathButton : BaseButton<MathView, MathPainter, Atom.MathList> { }
  public class TextButton : BaseButton<TextView, TextPainter, Rendering.Text.TextAtom> { }
}
internal class DefaultButtonStyle {
  internal const Aspect AspectFit = Aspect.AspectFit;
  internal static readonly Color TransparentBackground = Colors.Transparent;
}