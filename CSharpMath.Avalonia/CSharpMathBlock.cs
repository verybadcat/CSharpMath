using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using CSharpMath.Rendering;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using Typeface = Typography.OpenFont.Typeface;

namespace CSharpMath.Avalonia {
  public abstract class CSharpMathBlock<TPainter, TSource> : Control
    where TPainter : Painter<AvaloniaCanvas, TSource, Color>
    where TSource : struct, ISource {

    public static readonly StyledProperty<float> FontSizeProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TSource>, float>(nameof(FontSize));

    public static readonly StyledProperty<Color> ForegroundProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TSource>, Color>(nameof(Foreground));

    public static readonly DirectProperty<CSharpMathBlock<TPainter, TSource>, TSource> SourceProperty =
      AvaloniaProperty.RegisterDirect<CSharpMathBlock<TPainter, TSource>, TSource>(
        nameof(Source),
        block => block.Source,
        (block, source) => block.Source = source);

    public static readonly StyledProperty<AvaloniaTextAlignment> TextAlignmentProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TSource>, AvaloniaTextAlignment>(nameof(TextAlignment));

    public static readonly StyledProperty<IEnumerable<Typeface>> LocalTypefacesProperty =
      AvaloniaProperty.Register<CSharpMathBlock<TPainter, TSource>, IEnumerable<Typeface>>(nameof(LocalTypefaces));

    private static readonly IStyle DefaultStyle;

    static CSharpMathBlock() {
      FontSizeProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TSource>>(UpdateFontSize);
      ForegroundProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TSource>>(UpdateTextColor);
      LocalTypefacesProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TSource>>(UpdateLocalTypefaces);
      SourceProperty.Changed.AddClassHandler<CSharpMathBlock<TPainter, TSource>>(UpdateSource);

      AffectsMeasure<CSharpMathBlock<TPainter, TSource>>(
        FontSizeProperty, LocalTypefacesProperty, SourceProperty);

      AffectsRender<CSharpMathBlock<TPainter, TSource>>(
        FontSizeProperty, ForegroundProperty, LocalTypefacesProperty, SourceProperty, TextAlignmentProperty);

      DefaultStyle = new Style(s => s.Is<CSharpMathBlock<TPainter, TSource>>()) {
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

    protected abstract TSource Source { get; set; }

    public AvaloniaTextAlignment TextAlignment {
      get => GetValue(TextAlignmentProperty);
      set => SetValue(TextAlignmentProperty, value);
    }

    protected abstract TPainter Painter { get; }

    public override void Render(DrawingContext context) =>
        Painter.Draw(new AvaloniaCanvas(context, Bounds.Size), TextAlignment.ToCSharpMathTextAlignment());

    private static void UpdateFontSize(CSharpMathBlock<TPainter, TSource> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.FontSize = block.FontSize;

    private static void UpdateLocalTypefaces(CSharpMathBlock<TPainter, TSource> block, AvaloniaPropertyChangedEventArgs e) {
      block.Painter.LocalTypefaces.Clear();
      block.Painter.LocalTypefaces.AddRange(block.LocalTypefaces);
    }

    private static void UpdateSource(CSharpMathBlock<TPainter, TSource> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.Source = block.Source;

    private static void UpdateTextColor(CSharpMathBlock<TPainter, TSource> block, AvaloniaPropertyChangedEventArgs e) =>
      block.Painter.TextColor = block.Foreground;
  }
}
