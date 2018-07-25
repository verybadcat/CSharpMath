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
  using Color = Xamarin.Forms.Color;
  using TextAlignment = Rendering.TextAlignment;
  using Thickness = Rendering.Thickness;

  [XamlCompilation(XamlCompilationOptions.Compile), ContentProperty(nameof(LaTeX))]
  public class MathView : BaseView<SkiaSharp.MathPainter, MathSource>, IPainter<MathSource, Color> {
    #region BindableProperties
    static MathView() {
      var painter = new SkiaSharp.MathPainter();
      var thisType = typeof(MathView);
      StrokeCapProperty = BindableProperty.Create(nameof(StrokeCap), typeof(SKStrokeCap), thisType, painter.StrokeCap, propertyChanged: (b, o, n) => p(b).StrokeCap = (SKStrokeCap)n); \
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType,
        defaultValue: painter.GlyphBoxColor,
        propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.ToSKColor(), (((Color glyph, Color textRun)?)n).Value.textRun.ToSKColor()) : default((SKColor glyph, SKColor textRun)?));
    }
    public static readonly BindableProperty StrokeCapProperty;
    #endregion

    SKPoint _origin;
    protected override void OnTouch(SKTouchEventArgs e) {
      if (e.InContact) {
        switch (e.ActionType) {
          case SKTouchAction.Entered:
            break;
          case SKTouchAction.Pressed:
            _origin = e.Location;
            e.Handled = true;
            break;
          case SKTouchAction.Moved:
            var displacement = e.Location - _origin;
            DisplacementX += displacement.X;
            DisplacementY += displacement.Y;
            _origin = e.Location;
            InvalidateSurface();
            e.Handled = true;
            break;
          case SKTouchAction.Released:
            _origin = e.Location;
            e.Handled = true;
            break;
          case SKTouchAction.Cancelled:
            break;
          case SKTouchAction.Exited:
            break;
          default:
            break;
        }
      }
      base.OnTouch(e);
    }

    public SKStrokeCap StrokeCap { get => (SKStrokeCap)GetValue(StrokeCapProperty); set => SetValue(StrokeCapProperty, value); }
    public Interfaces.IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }

    public new RectangleF? Measure => painter.Measure;
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => painter.Measure is RectangleF r ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height)) : base.OnMeasure(widthConstraint, heightConstraint);
  }
}