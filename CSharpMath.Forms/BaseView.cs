using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Typography.OpenFont;
using Xamarin.Forms;

namespace CSharpMath.Forms {
  using Color = Color;
  using Rendering.FrontEnd;
  using Structures;

  public abstract class BaseView<TPainter, TContent> : SKCanvasView, IPainter<TContent, Color>
    where TPainter : Painter<SKCanvas, TContent, SKColor>, new() where TContent : class {
    public TPainter Painter { get; } = new TPainter();
    protected sealed override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      e.Surface.Canvas.Clear();
      Painter.Draw(e.Surface.Canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }

    SKPoint _origin;
    protected override void OnTouch(SKTouchEventArgs e) {
      if (e.InContact && !DisablePanning) {
        switch (e.ActionType) {
          case SKTouchAction.Pressed:
            _origin = e.Location;
            e.Handled = true;
            break;
          case SKTouchAction.Moved:
            var displacement = e.Location - _origin;
            _origin = e.Location;
            DisplacementX += displacement.X;
            DisplacementY += displacement.Y;
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

    public ObservableRangeCollection<Typeface> LocalTypefaces => Painter.LocalTypefaces;

    /// <summary>Panning is enabled by default when <see cref="SKCanvasView.EnableTouchEvents"/> is set to true.</summary>
    public bool DisablePanning { get => (bool)GetValue(DisablePanningProperty); set => SetValue(DisablePanningProperty, value); }
    public static readonly BindableProperty DisablePanningProperty = BindableProperty.Create(nameof(DisablePanning), typeof(bool), typeof(BaseView<TPainter, TContent>), false);

    #region Re-exposing Painter properties
    static readonly System.Reflection.ParameterInfo[] drawMethodParams = typeof(TPainter)
      .GetMethod(nameof(Painter<SKCanvas, TContent, SKColor>.Draw),
        new[] { typeof(SKCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float) }).GetParameters();
    protected static readonly TPainter staticPainter = new TPainter();
    protected static BindableProperty CreateProperty<T>(string propertyName, Func<TPainter, T> defaultValueGet, Action<TPainter, T> propertySet, Type? thisType = null) =>
      BindableProperty.Create(propertyName, typeof(T), thisType ?? typeof(BaseView<TPainter, TContent>), defaultValueGet(staticPainter),
        propertyChanged: (b, o, n) => {
          var p = (BaseView<TPainter, TContent>)b;
          propertySet(p.Painter, (T)n);
          p.InvalidateSurface(); // Redraw immediately! No deferred drawing
        });

    public string LaTeX { get => Painter.LaTeX; set => Painter.LaTeX = value; }
    public TContent? Content { get => (TContent)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly BindableProperty ContentProperty = CreateProperty(nameof(Content), p => p.Content, (p, v) => p.Content = v);
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty); set => SetValue(DisplayErrorInlineProperty, value); }
    public static readonly BindableProperty DisplayErrorInlineProperty = CreateProperty(nameof(DisplayErrorInline), p => p.DisplayErrorInline, (p, v) => p.DisplayErrorInline = v);
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public static readonly BindableProperty FontSizeProperty = CreateProperty(nameof(FontSize), p => p.FontSize, (p, v) => p.FontSize = v);
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public static readonly BindableProperty ErrorFontSizeProperty = CreateProperty(nameof(ErrorFontSize), p => p.ErrorFontSize, (p, v) => p.ErrorFontSize = v);
    public Color TextColor { get => (Color)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly BindableProperty TextColorProperty = CreateProperty(nameof(TextColor), p => p.TextColor, (p, v) => p.TextColor = v);
    public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public static readonly BindableProperty HighlightColorProperty = CreateProperty(nameof(HighlightColor), p => p.HighlightColor, (p, v) => p.HighlightColor = v);
    public Color ErrorColor { get => (Color)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public static readonly BindableProperty ErrorColorProperty = CreateProperty(nameof(ErrorColor), p => p.ErrorColor, (p, v) => p.ErrorColor = v);
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public static readonly BindableProperty TextAlignmentProperty = CreateProperty(nameof(TextAlignment), p => (TextAlignment)drawMethodParams[1].DefaultValue, (p, v) => { });
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public static readonly BindableProperty PaddingProperty = CreateProperty(nameof(Padding), p => (Thickness)(drawMethodParams[2].DefaultValue ?? new Thickness()), (p, v) => { });
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty); set => SetValue(DisplacementXProperty, value); }
    public static readonly BindableProperty DisplacementXProperty = CreateProperty(nameof(DisplacementX), p => (float)drawMethodParams[3].DefaultValue, (p, v) => { });
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty); set => SetValue(DisplacementYProperty, value); }
    public static readonly BindableProperty DisplacementYProperty = CreateProperty(nameof(DisplacementY), p => (float)drawMethodParams[4].DefaultValue, (p, v) => { });
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public static readonly BindableProperty MagnificationProperty = CreateProperty(nameof(Magnification), p => p.Magnification, (p, v) => p.Magnification = v);
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public static readonly BindableProperty PaintStyleProperty = CreateProperty(nameof(PaintStyle), p => p.PaintStyle, (p, v) => p.PaintStyle = v);
    public Atom.LineStyle LineStyle { get => (Atom.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public static readonly BindableProperty LineStyleProperty = CreateProperty(nameof(LineStyle), p => p.LineStyle, (p, v) => p.LineStyle = v);

    private static readonly BindablePropertyKey ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), typeof(BaseView<TPainter, TContent>), staticPainter.ErrorMessage, BindingMode.OneWayToSource);
    public static readonly BindableProperty ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }
    #endregion
  }
}