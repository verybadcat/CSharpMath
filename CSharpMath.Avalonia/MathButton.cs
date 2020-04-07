using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace CSharpMath.Avalonia {
  public class MathButton : Button {
    static MathButton() {
      LaTeXProperty.Changed.AddClassHandler<MathButton>((b, e) => b.Painter.LaTeX = (string)e.NewValue);
      AffectsMeasure<MathButton>(LaTeXProperty);
      AffectsRender<MathButton>(LaTeXProperty);
    }
    public MathPainter Painter { get; } = new MathPainter();
    protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize) =>
      Painter.Measure((float)availableSize.Width) is { } rect
      ? new global::Avalonia.Size(rect.Width, rect.Height)
      : base.MeasureOverride(availableSize);
    public override void Render(DrawingContext context) {
      Painter.Draw(new AvaloniaCanvas(context, Bounds.Size));
      base.Render(context);
    }
    public string? LaTeX { get => (string?)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    public static readonly AvaloniaProperty LaTeXProperty =
      AvaloniaProperty.Register<MathButton, string>(nameof(LaTeX));
  }
}
