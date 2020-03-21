using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Typography.OpenFont;
using Xamarin.Forms;

namespace CSharpMath.Forms {
  using Color = Color;
  using Rendering;
  using Structures;
  public abstract class BaseView<TPainter, TSource> : SKCanvasView, IPainter<TSource, Color>
    where TPainter : ICanvasPainter<SKCanvas, TSource, SKColor>, new()
    where TSource : ISource {
    public TPainter Painter { get; } = new TPainter();
    protected abstract TSource SourceFromLaTeX(string latex);
    protected abstract string LaTeXFromSource(TSource source);
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      e.Surface.Canvas.Clear();
      Painter.Draw(e.Surface.Canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }

    static readonly System.Reflection.ParameterInfo[] drawMethodParams = typeof(TPainter)
      .GetMethod(nameof(ICanvasPainter<SKCanvas, TSource, SKColor>.Draw),
        new[] { typeof(SKCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float) }).GetParameters();
    static readonly Type thisType = typeof(BaseView<TPainter, TSource>);
    static readonly TPainter staticPainter = new TPainter();
    static BaseView<TPainter, TSource> Actual(BindableObject b) => (BaseView<TPainter, TSource>)b;
    static TPainter P(BindableObject b) => Actual(b).Painter;
    public static readonly BindableProperty DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, staticPainter.DisplayErrorInline, propertyChanged: (b, o, n) => P(b).DisplayErrorInline = (bool)n);
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType, staticPainter.FontSize, propertyChanged: (b, o, n) => P(b).FontSize = (float)n);
    public static readonly BindableProperty ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType, staticPainter.ErrorFontSize, propertyChanged: (b, o, n) => P(b).ErrorFontSize = (float)n);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType, staticPainter.TextColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).TextColor = ((Color)n).ToSKColor());
    public static readonly BindableProperty HighlightColorProperty = BindableProperty.Create(nameof(HighlightColor), typeof(Color), thisType, staticPainter.HighlightColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).HighlightColor = ((Color)n).ToSKColor());
    public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType, staticPainter.ErrorColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).ErrorColor = ((Color)n).ToSKColor());
    public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(nameof(TextAlignment), typeof(TextAlignment), thisType, drawMethodParams[1].DefaultValue is DBNull ? default(TextAlignment) : drawMethodParams[1].DefaultValue ?? default(TextAlignment));
    public static readonly BindableProperty DisplacementXProperty = BindableProperty.Create(nameof(DisplacementX), typeof(float), thisType, drawMethodParams[3].DefaultValue, BindingMode.TwoWay);
    public static readonly BindableProperty DisplacementYProperty = BindableProperty.Create(nameof(DisplacementY), typeof(float), thisType, drawMethodParams[4].DefaultValue, BindingMode.TwoWay);
    public static readonly BindableProperty MagnificationProperty = BindableProperty.Create(nameof(Magnification), typeof(float), thisType, staticPainter.Magnification, propertyChanged: (b, o, n) => P(b).Magnification = (float)n);
    public static readonly BindableProperty PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(PaintStyle), thisType, staticPainter.PaintStyle, propertyChanged: (b, o, n) => P(b).PaintStyle = (PaintStyle)n);
    public static readonly BindableProperty LineStyleProperty = BindableProperty.Create(nameof(LineStyle), typeof(Atom.LineStyle), thisType, staticPainter.LineStyle, propertyChanged: (b, o, n) => P(b).LineStyle = (Atom.LineStyle)n);
    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType, drawMethodParams[2].DefaultValue ?? default(Thickness));
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(TSource), thisType, staticPainter.Source, BindingMode.TwoWay, null, (b, o, n) => { P(b).Source = (TSource)n; Actual(b).ErrorMessage = P(b).ErrorMessage; var latex = Actual(b).LaTeXFromSource((TSource)n); if (Actual(b).LaTeX != latex) Actual(b).LaTeX = latex; });
    private static readonly BindablePropertyKey ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, staticPainter.ErrorMessage, BindingMode.OneWayToSource);
    public static readonly BindableProperty ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
    public static readonly BindableProperty LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, staticPainter.LaTeX, propertyChanged: (b, o, n) => Actual(b).Source = Actual(b).SourceFromLaTeX((string)n));
    public static readonly BindableProperty DisablePanningProperty = BindableProperty.Create(nameof(DisablePanning), typeof(bool), thisType, false);

    SKPoint _origin;
    protected override void OnTouch(SKTouchEventArgs e) {
      if (e.InContact && Source.IsValid && !DisablePanning) {
        switch (e.ActionType) {
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
          case SKTouchAction.Entered:
          case SKTouchAction.Cancelled:
          case SKTouchAction.Exited:
          default:
            break;
        }
      }
      base.OnTouch(e);
    }

    public TSource Source { get => (TSource)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty); set => SetValue(DisplayErrorInlineProperty, value); }
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public Color ErrorColor { get => (Color)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty); set => SetValue(DisplacementXProperty, value); }
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty); set => SetValue(DisplacementYProperty, value); }
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public Atom.LineStyle LineStyle { get => (Atom.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }
    public ObservableRangeCollection<Typeface> LocalTypefaces => Painter.LocalTypefaces;
    public string LaTeX { get => LaTeXFromSource(Source); set => Source = SourceFromLaTeX(value); }
    /// <summary>Panning is enabled by default when <see cref="SKCanvasView.EnableTouchEvents"/> is set to true.</summary>
    public bool DisablePanning { get => (bool)GetValue(DisablePanningProperty); set => SetValue(DisablePanningProperty, value); }
  }
}