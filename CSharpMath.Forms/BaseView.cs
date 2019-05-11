using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
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

  public interface IPainterAndSourceSupplier<TPainter, TSource> {
    TPainter Default { get; }
    TSource SourceFromLaTeX(string latex);
    string LaTeXFromSource(TSource source);
    string DefaultLaTeX(TPainter painter);
  }

  public abstract class BaseView<TPainter, TSource, TPainterAndSourceSupplier> : SKCanvasView, IPainter<TSource, Color> where TPainter : ICanvasPainter<SKCanvas, TSource, SKColor> where TSource : struct, ISource where TPainterAndSourceSupplier : struct, IPainterAndSourceSupplier<TPainter, TSource> {
    public BaseView(TPainter painter) => Painter = painter;

    public TPainter Painter { get; }
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      e.Surface.Canvas.Clear();
      Painter.Draw(e.Surface.Canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }

    #region BindableProperties
    static BaseView() {
      var supplier = default(TPainterAndSourceSupplier);
      var painter = supplier.Default;
      var thisType = typeof(BaseView<TPainter, TSource, TPainterAndSourceSupplier>);
      BaseView<TPainter, TSource, TPainterAndSourceSupplier> Actual(BindableObject b) => (BaseView<TPainter, TSource, TPainterAndSourceSupplier>)b;
      var drawMethodParams = typeof(TPainter).GetMethod(nameof(ICanvasPainter<SKCanvas, TSource, SKColor>.Draw), new[] { typeof(SKCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float) }).GetParameters();
      TPainter p(BindableObject b) => ((BaseView<TPainter, TSource, TPainterAndSourceSupplier>)b).Painter;
      SourceProperty = BindableProperty.Create(nameof(Source), typeof(TSource), thisType, painter.Source, BindingMode.TwoWay, null,
        (b, o, n) => { p(b).Source = (TSource)n; Actual(b).ErrorMessage = p(b).ErrorMessage; var latex = supplier.LaTeXFromSource((TSource)n); if(Actual(b).LaTeX != latex || false/*get rid of warning for redundant check*/) Actual(b).LaTeX = latex; });
      DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, painter.DisplayErrorInline, propertyChanged: (b, o, n) => p(b).DisplayErrorInline = (bool)n);
      FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType, painter.FontSize, propertyChanged: (b, o, n) => p(b).FontSize = (float)n);
      ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType, painter.ErrorFontSize, propertyChanged: (b, o, n) => p(b).ErrorFontSize = (float)n);
      TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType, painter.TextColor.ToFormsColor(), propertyChanged: (b, o, n) => p(b).TextColor = ((Color)n).ToSKColor());
      HighlightColorProperty = BindableProperty.Create(nameof(HighlightColor), typeof(Color), thisType, painter.HighlightColor.ToFormsColor(), propertyChanged: (b, o, n) => p(b).HighlightColor = ((Color)n).ToSKColor());
      ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType, painter.ErrorColor.ToFormsColor(), propertyChanged: (b, o, n) => p(b).ErrorColor = ((Color)n).ToSKColor());
      TextAlignmentProperty = BindableProperty.Create(nameof(TextAlignment), typeof(TextAlignment), thisType, drawMethodParams[1].DefaultValue is DBNull ? default(TextAlignment) : drawMethodParams[1].DefaultValue ?? default(TextAlignment));
      DisplacementXProperty = BindableProperty.Create(nameof(DisplacementX), typeof(float), thisType, drawMethodParams[3].DefaultValue, BindingMode.TwoWay);
      DisplacementYProperty = BindableProperty.Create(nameof(DisplacementY), typeof(float), thisType, drawMethodParams[4].DefaultValue, BindingMode.TwoWay);
      MagnificationProperty = BindableProperty.Create(nameof(Magnification), typeof(float), thisType, painter.Magnification, propertyChanged: (b, o, n) => p(b).Magnification = (float)n);
      PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(PaintStyle), thisType, painter.PaintStyle, propertyChanged: (b, o, n) => p(b).PaintStyle = (PaintStyle)n);
      LineStyleProperty = BindableProperty.Create(nameof(LineStyle), typeof(Enumerations.LineStyle), thisType, painter.LineStyle, propertyChanged: (b, o, n) => p(b).LineStyle = (Enumerations.LineStyle)n);
      PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType, drawMethodParams[2].DefaultValue ?? default(Thickness));
      ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, painter.ErrorMessage, BindingMode.OneWayToSource);
      ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
      LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, supplier.DefaultLaTeX(painter),
        propertyChanged: (b, o, n) => { var ss = supplier.SourceFromLaTeX((string)n); Actual(b).Source = ss; });
    }
    public static readonly BindableProperty DisplayErrorInlineProperty;
    public static readonly BindableProperty FontSizeProperty;
    public static readonly BindableProperty ErrorFontSizeProperty;
    public static readonly BindableProperty TextColorProperty;
    public static readonly BindableProperty HighlightColorProperty;
    public static readonly BindableProperty ErrorColorProperty;
    public static readonly BindableProperty TextAlignmentProperty;
    public static readonly BindableProperty DisplacementXProperty;
    public static readonly BindableProperty DisplacementYProperty;
    public static readonly BindableProperty MagnificationProperty;
    public static readonly BindableProperty PaintStyleProperty;
    public static readonly BindableProperty LineStyleProperty;
    public static readonly BindableProperty PaddingProperty;
    public static readonly BindableProperty SourceProperty;
    private static readonly BindablePropertyKey ErrorMessagePropertyKey;
    public static readonly BindableProperty ErrorMessageProperty;
    public static readonly BindableProperty LaTeXProperty;
    #endregion

    SKPoint _origin;
    protected override void OnTouch(SKTouchEventArgs e) {
      if (e.InContact && Source.IsValid) {
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
    
    public TSource Source { get => (TSource)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty); set => SetValue(DisplayErrorInlineProperty, value); }
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public Color ErrorColor { get => (Color)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty); set => SetValue(DisplacementXProperty, value); }
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty); set => SetValue(DisplacementYProperty, value); }
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public Enumerations.LineStyle LineStyle { get => (Enumerations.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }
    public ObservableRangeCollection<Typeface> LocalTypefaces => Painter.LocalTypefaces;
    public string LaTeX { get => default(TPainterAndSourceSupplier).LaTeXFromSource(Source); set => Source = default(TPainterAndSourceSupplier).SourceFromLaTeX(value); }
  }
}
