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
    where TPainter : Painter<SKCanvas, TSource, SKColor>, new()
    where TSource : ISource {
    public TPainter Painter { get; } = new TPainter();
    protected abstract TSource SourceFromLaTeX(string latex);
    protected abstract string LaTeXFromSource(TSource source);
    protected sealed override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      e.Surface.Canvas.Clear();
      Painter.Draw(e.Surface.Canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }

    SKPoint _origin;
    protected override void OnTouch(SKTouchEventArgs e) {
      if (e.InContact && (Source?.IsValid ?? false) && !DisablePanning) {
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

    static BindableProperty CreateProperty<T>(string propertyName, Func<TPainter, T> getter, Action<TPainter, T> setter) =>
      BindableProperty.Create(propertyName, typeof(T), typeof(BaseView<TPainter, TSource>), getter(staticPainter),
        propertyChanged: (b, o, n) => setter(((BaseView<TPainter, TSource>)b).Painter, (T)n));

    static readonly System.Reflection.ParameterInfo[] drawMethodParams = typeof(TPainter)
      .GetMethod(nameof(Painter<SKCanvas, TSource, SKColor>.Draw),
        new[] { typeof(SKCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float) }).GetParameters();
    static readonly Type thisType = typeof(BaseView<TPainter, TSource>);
    protected static readonly TPainter staticPainter = new TPainter();
    protected static BaseView<TPainter, TSource> Actual(BindableObject b) => (BaseView<TPainter, TSource>)b;
    protected static TPainter P(BindableObject b) => Actual(b).Painter;

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(TSource), thisType, staticPainter.Source, BindingMode.TwoWay, null, (b, o, n) => { P(b).Source = (TSource)n; Actual(b).ErrorMessage = P(b).ErrorMessage; var latex = Actual(b).LaTeXFromSource((TSource)n); if (Actual(b).LaTeX != latex) Actual(b).LaTeX = latex; });
    public TSource Source { get => (TSource)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public static readonly BindableProperty DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, staticPainter.DisplayErrorInline, propertyChanged: (b, o, n) => P(b).DisplayErrorInline = (bool)n);
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty); set => SetValue(DisplayErrorInlineProperty, value); }
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType, staticPainter.FontSize, propertyChanged: (b, o, n) => P(b).FontSize = (float)n);
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public static readonly BindableProperty ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType, staticPainter.ErrorFontSize, propertyChanged: (b, o, n) => P(b).ErrorFontSize = (float)n);
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType, staticPainter.TextColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).TextColor = ((Color)n).ToSKColor());
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly BindableProperty HighlightColorProperty = BindableProperty.Create(nameof(HighlightColor), typeof(Color), thisType, staticPainter.HighlightColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).HighlightColor = ((Color)n).ToSKColor());
    public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType, staticPainter.ErrorColor.ToFormsColor(), propertyChanged: (b, o, n) => P(b).ErrorColor = ((Color)n).ToSKColor());
    public Color ErrorColor { get => (Color)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(nameof(TextAlignment), typeof(TextAlignment), thisType, drawMethodParams[1].DefaultValue is DBNull ? default(TextAlignment) : drawMethodParams[1].DefaultValue ?? default(TextAlignment));
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public static readonly BindableProperty DisplacementXProperty = BindableProperty.Create(nameof(DisplacementX), typeof(float), thisType, drawMethodParams[3].DefaultValue, BindingMode.TwoWay);
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty); set => SetValue(DisplacementXProperty, value); }
    public static readonly BindableProperty DisplacementYProperty = BindableProperty.Create(nameof(DisplacementY), typeof(float), thisType, drawMethodParams[4].DefaultValue, BindingMode.TwoWay);
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty); set => SetValue(DisplacementYProperty, value); }
    public static readonly BindableProperty MagnificationProperty = BindableProperty.Create(nameof(Magnification), typeof(float), thisType, staticPainter.Magnification, propertyChanged: (b, o, n) => P(b).Magnification = (float)n);
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public static readonly BindableProperty PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(PaintStyle), thisType, staticPainter.PaintStyle, propertyChanged: (b, o, n) => P(b).PaintStyle = (PaintStyle)n);
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public static readonly BindableProperty LineStyleProperty = BindableProperty.Create(nameof(LineStyle), typeof(Atom.LineStyle), thisType, staticPainter.LineStyle, propertyChanged: (b, o, n) => P(b).LineStyle = (Atom.LineStyle)n);
    public Atom.LineStyle LineStyle { get => (Atom.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType, drawMethodParams[2].DefaultValue ?? default(Thickness));
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public static readonly BindableProperty ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
    private static readonly BindablePropertyKey ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, staticPainter.ErrorMessage, BindingMode.OneWayToSource);
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }
    public ObservableRangeCollection<Typeface> LocalTypefaces => Painter.LocalTypefaces;
    public static readonly BindableProperty LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, staticPainter.LaTeX, propertyChanged: (b, o, n) => Actual(b).Source = Actual(b).SourceFromLaTeX((string)n));
    public string LaTeX { get => LaTeXFromSource(Source); set => Source = SourceFromLaTeX(value); }
    public static readonly BindableProperty DisablePanningProperty = BindableProperty.Create(nameof(DisablePanning), typeof(bool), thisType, false);
    /// <summary>Panning is enabled by default when <see cref="SKCanvasView.EnableTouchEvents"/> is set to true.</summary>
    public bool DisablePanning { get => (bool)GetValue(DisablePanningProperty); set => SetValue(DisablePanningProperty, value); }
  }
}