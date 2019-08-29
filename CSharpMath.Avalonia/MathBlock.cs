using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia {
  public class MathBlock : CSharpMathBlock<MathPainter<AvaloniaCanvas, Color>, MathSource> {
    private MathSource _source;

    public MathBlock() {
      Painter = new MathPainter();
    }

    protected override MathPainter<AvaloniaCanvas, Color> Painter { get; }

    [TypeConverter(typeof(MathSourceTypeConverter))]
    protected override MathSource Source {
      get => _source;
      set => SetAndRaise(SourceProperty, ref _source, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
      Painter.Measure?.Size.ToAvaloniaSize() ?? Size.Empty;
  }
}
