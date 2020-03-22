using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  using CSharpMath.SkiaSharp;
  using Color = Xamarin.Forms.Color;

  [XamlCompilation(XamlCompilationOptions.Compile)]
  public class MathView : BaseView<MathPainter, MathSource>, IPainter<MathSource, Color> {
    protected override MathSource SourceFromLaTeX(string latex) => MathSource.FromLaTeX(latex);
    protected override string LaTeXFromSource(MathSource source) => source.LaTeX;
    
    public new RectangleF? Measure => Painter.Measure();
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) =>
      Painter.Measure() is RectangleF r
      ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height))
      : base.OnMeasure(widthConstraint, heightConstraint);

    static readonly Type thisType = typeof(MathView);
    public Atom.MathList MathList { get => Source.MathList; set => Source = new MathSource(value); }

    public static readonly BindableProperty StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, staticPainter.StrokeCap, propertyChanged: (b, o, n) => P(b).StrokeCap = (SKStrokeCap)n);
    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public static readonly BindableProperty GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType, staticPainter.GlyphBoxColor,
        propertyChanged: (b, o, n) => P(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
  }
}