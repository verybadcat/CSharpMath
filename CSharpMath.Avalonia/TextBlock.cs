using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CSharpMath.Rendering.FrontEnd;
using CSharpMath.Rendering.Text;

namespace CSharpMath.Avalonia {
  public class TextBlock : CSharpMathBlock<TextPainter<AvaloniaCanvas, Color>, TextAtom> {
    private TextAtom? _content;

    public TextBlock() {
      Painter = new TextPainter();
    }

    protected override TextPainter<AvaloniaCanvas, Color> Painter { get; }

    [TypeConverter(typeof(TextSourceTypeConverter))]
    public override TextAtom? Content {
      get => _content;
      set => SetAndRaise(ContentProperty, ref _content, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
      Painter.Measure((float)availableSize.Width)?.Size.ToAvaloniaSize() ?? Size.Empty;
  }
}
