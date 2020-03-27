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
  public abstract class CSharpMathBlock<TPainter, TContent> : Control
    where TPainter : Painter<AvaloniaCanvas, TContent, Color> where TContent : class {

    public static readonly StyledProperty<float> FontSizeProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TContent>, float>(nameof(FontSize));

    public static readonly StyledProperty<Color> ForegroundProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TContent>, Color>(nameof(Foreground));

    public static readonly DirectProperty<CSharpMathBlock<TPainter, TContent>, TContent?> ContentProperty =
      AvaloniaProperty.RegisterDirect<CSharpMathBlock<TPainter, TContent>, TContent?>(
        nameof(Content),
        block => block.Content,
        (block, content) => block.Content = content);

    public static readonly StyledProperty<AvaloniaTextAlignment> TextAlignmentProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TContent>, AvaloniaTextAlignment>(nameof(TextAlignment));

    public static readonly StyledProperty<IEnumerable<Typeface>> LocalTypefacesProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces));

    private static readonly IStyle DefaultStyle;

    static CSharpMathBlock() {
      FontSizeProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TContent>>(UpdateFontSize);
      ForegroundProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TContent>>(UpdateTextColor);
      LocalTypefacesProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TContent>>(UpdateLocalTypefaces);
      ContentProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TContent>>(UpdateContent);

      AffectsMeasure<CSharpMathBlock<TPainter, TContent>>(
        FontSizeProperty, LocalTypefacesProperty, ContentProperty);

      AffectsRender<CSharpMathBlock<TPainter, TContent>>(
        FontSizeProperty, ForegroundProperty, LocalTypefacesProperty, ContentProperty, TextAlignmentProperty);

      DefaultStyle = new Style(s => s.Is<CSharpMathBlock<TPainter, TContent>>()) {
        Setters = new Setter[]
        {
          new Setter(ForegroundProperty, new DynamicResourceExtension("ThemeForegroundColor")),
          new Setter(FontSizeProperty, new DynamicResourceExtension("FontSizeNormal"))
        }
      };
    }

    public CSharpMathBlock() {
      Styles.Add(DefaultStyle);
    }

    public float FontSize {
      get => GetValue(FontSizeProperty);
      set => SetValue(FontSizeProperty, value);
    }

    public Color Foreground {
      get => GetValue(ForegroundProperty);
      set => SetValue(ForegroundProperty, value);
    }

    public IEnumerable<Typeface> LocalTypefaces {
      get => GetValue(LocalTypefacesProperty);
      set => SetValue(LocalTypefacesProperty, value);
    }

    public abstract TContent? Content { get; set; }

    public AvaloniaTextAlignment TextAlignment {
      get => GetValue(TextAlignmentProperty);
      set => SetValue(TextAlignmentProperty, value);
    }

    protected abstract TPainter Painter { get; }

    public override void Render(DrawingContext context) =>
        Painter.Draw(new AvaloniaCanvas(context, Bounds.Size), TextAlignment.ToCSharpMathTextAlignment());

    private static void UpdateFontSize(CSharpMathBlock<TPainter, TContent> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.FontSize = block.FontSize;

    private static void UpdateLocalTypefaces(CSharpMathBlock<TPainter, TContent> block, AvaloniaPropertyChangedEventArgs e) {
      block.Painter.LocalTypefaces.Clear();
      block.Painter.LocalTypefaces.AddRange(block.LocalTypefaces);
    }

    private static void UpdateContent(CSharpMathBlock<TPainter, TContent> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.Content = block.Content;

    private static void UpdateTextColor(CSharpMathBlock<TPainter, TContent> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.TextColor = block.Foreground;
  }
}
