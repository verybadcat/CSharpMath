using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Rendering.Renderer;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  using CSharpMath.SkiaSharp;
  using Color = Xamarin.Forms.Color;

  [XamlCompilation(XamlCompilationOptions.Compile)]
  public class MathView : BaseView<MathPainter, MathSource, MathView.PainterSupplier>, IPainter<MathSource, Color> {
    public struct PainterSupplier : IPainterAndSourceSupplier<MathPainter, MathSource> {
      public MathPainter Default => new MathPainter();
      public string DefaultLaTeX(MathPainter painter) => painter.LaTeX;
      public string LaTeXFromSource(MathSource source) => source.LaTeX;
      public MathSource SourceFromLaTeX(string latex) => MathSource.FromLaTeX(latex);
    }
    public MathView() : base(default(PainterSupplier).Default) { }
    #region BindableProperties
    static MathView() {
      var painter = default(PainterSupplier).Default;
      var thisType = typeof(MathView);

      static MathPainter p(BindableObject b) => ((MathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType, painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    }
    public static readonly BindableProperty StrokeCapProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    #endregion
    public SKStrokeCap StrokeCap {
      get => (SKStrokeCap)GetValue(StrokeCapProperty);
      set => SetValue(StrokeCapProperty, value);
    }
    public (Color glyph, Color textRun)? GlyphBoxColor {
      get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty);
      set => SetValue(GlyphBoxColorProperty, value);
    }
    public Atoms.MathList MathList {
      get => Source.MathList;
      set => Source = new MathSource(value);
    }
    public new RectangleF? Measure => Painter.Measure();
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) =>
      Painter.Measure() is RectangleF r
      ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height))
      : base.OnMeasure(widthConstraint, heightConstraint);
  }
}