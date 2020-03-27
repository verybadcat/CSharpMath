using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using CSharpMath.Rendering.FrontEnd;

namespace CSharpMath.Avalonia {
  public class MathBlock : CSharpMathBlock<MathPainter<AvaloniaCanvas, Color>, Atom.MathList> {
    private Atom.MathList? _content;

    public MathBlock() {
      Painter = new MathPainter();
    }

    protected override MathPainter<AvaloniaCanvas, Color> Painter { get; }

    [TypeConverter(typeof(MathTypeConverter))]
    public override Atom.MathList? Content {
      get => _content;
      set => SetAndRaise(ContentProperty, ref _content, value);
    }

    protected override Size MeasureOverride(Size availableSize) =>
      Painter.Measure()?.Size.ToAvaloniaSize() ?? Size.Empty;
  }
}
