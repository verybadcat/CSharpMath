using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Typography.OpenFont;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CSharpMath.Forms {
  using System.Runtime.CompilerServices;
  using CSharpMath.SkiaSharp;
  using Color = Xamarin.Forms.Color;

  [XamlCompilation(XamlCompilationOptions.Compile)]
  public class MathView : BaseView<MathPainter, MathSource, MathView.PainterSupplier>, IPainter<MathSource, Color> {
    public struct PainterSupplier : IPainterSupplier<MathPainter> {
      public MathPainter Default => new MathPainter();
    }
    public MathView() : base(default(PainterSupplier).Default) { }

    //Yes, an inefficient hack to correlate the two properties...
    #region BindableProperties
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      base.OnPropertyChanged(propertyName);
      if (propertyName == LaTeXProperty.PropertyName) {
        if (LaTeX is null) {
          if (Source.IsValid) Source = new MathSource();
        } else {
          var desiredTarget = new MathSource(LaTeX);
          if (!Source.Equals(desiredTarget))
            Source = desiredTarget;
        }
      } else if (propertyName == SourceProperty.PropertyName) {
        if (Source.IsValid) {
          var desiredTarget = Source.LaTeX;
          if (!LaTeX.Equals(desiredTarget))
            LaTeX = desiredTarget;
        } else if (!string.IsNullOrEmpty(LaTeX)) LaTeX = "";
      }
    }
    static MathView() {
      var painter = default(PainterSupplier).Default;
      var thisType = typeof(MathView);
      MathPainter p(BindableObject b) => ((MathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType,
        defaultValue: painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
      LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, painter.LaTeX);
    }
    public static readonly BindableProperty StrokeCapProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    public static readonly BindableProperty LaTeXProperty;
    #endregion

    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public Interfaces.IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => (string)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    public new RectangleF? Measure => Painter.Measure;
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => Painter.Measure is RectangleF r ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height)) : base.OnMeasure(widthConstraint, heightConstraint);

  }
}