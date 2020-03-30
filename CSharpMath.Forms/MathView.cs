using System.Drawing;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  using CSharpMath.SkiaSharp;
  using Color = Xamarin.Forms.Color;

  [XamlCompilation(XamlCompilationOptions.Compile)]
  public class MathView : BaseView<MathPainter, Atom.MathList> {
    public new RectangleF? Measure => Painter.Measure();
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) =>
      Painter.Measure() is RectangleF r
      ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height))
      : base.OnMeasure(widthConstraint, heightConstraint);

    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public static readonly BindableProperty StrokeCapProperty = CreateProperty(nameof(StrokeCap), p => p.StrokeCap, (p, v) => p.StrokeCap = v, typeof(MathView));
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public static readonly BindableProperty GlyphBoxColorProperty = CreateProperty(nameof(GlyphBoxColor), p => p.GlyphBoxColor, (p, v) => p.GlyphBoxColor = v, typeof(MathView));
  }
}