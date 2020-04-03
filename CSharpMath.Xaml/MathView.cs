#if Avalonia
using XCanvas = CSharpMath.Avalonia.AvaloniaCanvas;
using XCanvasColor = Avalonia.Media.Color;
using XColor = Avalonia.Media.Color;
using XControl = Avalonia.Controls.Control;
using XProperty = Avalonia.AvaloniaProperty;
namespace CSharpMath.Avalonia {
#elif Forms
using SkiaSharp;
using XCanvas = SkiaSharp.SKCanvas;
using XCanvasColor = SkiaSharp.SKColor;
using XColor = Xamarin.Forms.Color;
using XControl = SkiaSharp.Views.Forms.SKCanvasView;
using XProperty = Xamarin.Forms.BindableProperty;
namespace CSharpMath.Forms {
  using MathPainter = SkiaSharp.MathPainter;
#endif
  public class MathView : BaseView<MathPainter, Atom.MathList> {
#if Forms
    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public static readonly XProperty StrokeCapProperty = CreateProperty<MathView, SKStrokeCap>(nameof(StrokeCap), false, p => p.StrokeCap, (p, v) => p.StrokeCap = v);
#endif
  }
}