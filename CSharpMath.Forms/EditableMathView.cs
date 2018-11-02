using System;
using System.Drawing;
using CSharpMath.Rendering;
using SkiaSharp;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using static SkiaSharp.Views.Forms.Extensions;

namespace CSharpMath.Forms {
  public class Button : Editor.IButton {
    public Xamarin.Forms.Button Content { get; set; }
    public string Text { get => Content.Text; set => Content.Text = value; }
    public bool Enabled { get => Content.IsEnabled; set => Content.IsEnabled = value; }
    public bool Selected { get => throw null; set => throw null; }
  }
  public class EditableMathView : BaseView<SkiaSharp.EditableMathPainter<Button>, MathSource>, IPainter<MathSource, Color> {
    #region BindableProperties
    static EditableMathView() {
      var painter = new SkiaSharp.EditableMathPainter<Button>(null);
      var thisType = typeof(MathView);
      SkiaSharp.EditableMathPainter<Button> p(BindableObject b) => ((EditableMathView)b).Painter;
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType,
        defaultValue: painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    }
    public static readonly BindableProperty StrokeCapProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    #endregion

    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public Interfaces.IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
    public new RectangleF? Measure => Painter.Measure;
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => Painter.Measure is RectangleF r ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height)) : base.OnMeasure(widthConstraint, heightConstraint);

    public ContentView Default =>
      Painter.
  }
}