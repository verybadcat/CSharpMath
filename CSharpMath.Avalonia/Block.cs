using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using CSharpMath.Rendering.FrontEnd;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using Typeface = Typography.OpenFont.Typeface;

namespace CSharpMath.Avalonia {
  public class MathBlock : BaseBlock<MathPainter, Atom.MathList> { }
  public class TextBlock : BaseBlock<TextPainter, Rendering.Text.TextAtom> { }
  public abstract class BaseBlock<TPainter, TContent> : Control
    where TPainter : Painter<AvaloniaCanvas, TContent, Color>, new() where TContent : class {
    static BaseBlock() {
      FontSizeProperty.Changed.AddClassHandler<BaseBlock<TPainter, TContent>>((b, e) => b.Painter.FontSize = b.FontSize);
      ForegroundProperty.Changed.AddClassHandler<BaseBlock<TPainter, TContent>>((b, e) => b.Painter.TextColor = b.Foreground);
      ContentProperty.Changed.AddClassHandler<BaseBlock<TPainter, TContent>>((b, e) => b.Painter.Content = b.Content);
      LocalTypefacesProperty.Changed.AddClassHandler<BaseBlock<TPainter, TContent>>((b, e) => {
        b.Painter.LocalTypefaces.Clear();
        b.Painter.LocalTypefaces.AddRange(b.LocalTypefaces);
      });

      AffectsMeasure<BaseBlock<TPainter, TContent>>(
        ContentProperty, FontSizeProperty, LocalTypefacesProperty
      );
      AffectsRender<BaseBlock<TPainter, TContent>>(
        ContentProperty, FontSizeProperty, LocalTypefacesProperty,
        ForegroundProperty, TextAlignmentProperty
      );

      DefaultStyle = new Style(s => s.Is<BaseBlock<TPainter, TContent>>()) {
        Setters = new Setter[]
        {
          new Setter(ForegroundProperty, new DynamicResourceExtension("ThemeForegroundColor")),
          new Setter(FontSizeProperty, new DynamicResourceExtension("FontSizeNormal"))
        }
      };
    }
    private static readonly IStyle DefaultStyle;
    public BaseBlock() => Styles.Add(DefaultStyle);
    protected TPainter Painter { get; } = new TPainter();
    protected override Size MeasureOverride(Size availableSize) =>
      Painter.Measure((float)availableSize.Width) is { } rect ? new Size(rect.Width, rect.Height) : Size.Empty;
    public override void Render(DrawingContext context) =>
      Painter.Draw(new AvaloniaCanvas(context, Bounds.Size), TextAlignment.ToCSharpMathTextAlignment());
    /// <summary>For databinding, bind to <see cref="Content"/>.</summary>
    public string LaTeX {
      get => (string)System.ComponentModel.TypeDescriptor.GetConverter(typeof(TContent)).ConvertTo(Content, typeof(string));
      set => Content = (TContent)System.ComponentModel.TypeDescriptor.GetConverter(typeof(TContent)).ConvertFrom(value);
    }
    public TContent? Content { get => GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly StyledProperty<TContent?> ContentProperty =
      AvaloniaProperty.Register<BaseBlock<TPainter, TContent>, TContent?>(nameof(Content));
    public float FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public static readonly StyledProperty<float> FontSizeProperty =
      AvaloniaProperty.Register<BaseBlock<TPainter, TContent>, float>(nameof(FontSize));
    public Color Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }
    public static readonly StyledProperty<Color> ForegroundProperty =
      AvaloniaProperty.Register<BaseBlock<TPainter, TContent>, Color>(nameof(Foreground));
    public IEnumerable<Typeface> LocalTypefaces { get => GetValue(LocalTypefacesProperty); set => SetValue(LocalTypefacesProperty, value); }
    public static readonly StyledProperty<IEnumerable<Typeface>> LocalTypefacesProperty =
      AvaloniaProperty.Register<BaseBlock<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces));
    public AvaloniaTextAlignment TextAlignment { get => GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public static readonly StyledProperty<AvaloniaTextAlignment> TextAlignmentProperty =
      AvaloniaProperty.Register<BaseBlock<TPainter, TContent>, AvaloniaTextAlignment>(nameof(TextAlignment));
  }
}
