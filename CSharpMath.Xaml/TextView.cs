using CSharpMath.Rendering.Text;
#if Avalonia
using XProperty = Avalonia.AvaloniaProperty;
using XCanvas = CSharpMath.Avalonia.AvaloniaCanvas;
using XCanvasColor = Avalonia.Media.Color;
using XColor = Avalonia.Media.Color;
using XControl = Avalonia.Controls.Control;
namespace CSharpMath.Avalonia {
#elif Forms
using SkiaSharp;
using XProperty = Xamarin.Forms.BindableProperty;
using XCanvas = SkiaSharp.SKCanvas;
using XCanvasColor = SkiaSharp.SKColor;
using XColor = Xamarin.Forms.Color;
using XControl = SkiaSharp.Views.Forms.SKCanvasView;
namespace CSharpMath.Forms {
  using TextPainter = SkiaSharp.TextPainter;
#endif
  public class TextView : BaseView<TextPainter, TextAtom> {
    public float AdditionalLineSpacing { get => (float)GetValue(AdditionalLineSpacingProperty); set => SetValue(AdditionalLineSpacingProperty, value); }
    public static readonly XProperty AdditionalLineSpacingProperty = CreateProperty<TextView, float>(nameof(AdditionalLineSpacing), true, p => p.AdditionalLineSpacing, (p, v) => p.AdditionalLineSpacing = v);
  }
}