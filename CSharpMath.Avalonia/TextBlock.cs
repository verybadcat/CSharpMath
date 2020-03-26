using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia {
  public class TextBlock : CSharpMathBlock<TextPainter<AvaloniaCanvas, Color>, TextSource> {
    private TextSource _source;

    public TextBlock() {
      Painter = new TextPainter();
    }

    protected override TextPainter<AvaloniaCanvas, Color> Painter { get; }

    [TypeConverter(typeof(TextSourceTypeConverter))]
    protected override TextSource Source {
      get => _source;
      set => SetAndRaise(SourceProperty, ref _source, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
      Painter.Measure((float)availableSize.Width)?.Size.ToAvaloniaSize() ?? Size.Empty;
  }
}
