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

  public abstract class BaseView<TPainter, TSource> : SKCanvasView, IPainter<TSource, Color> where TPainter : ICanvasPainter<SKCanvas, TSource, SKColor> where TSource : struct, ISource {
    public BaseView() {
      Painter = (TPainter)painterCtor.Invoke(ctorParams);
      //TODO: Until fixed...
      /*var pinch = new PinchGestureRecognizer();
      pinch.PinchUpdated += OnPinch;
      GestureRecognizers.Add(pinch);*/
    }

    protected TPainter Painter { get; }
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      e.Surface.Canvas.Clear();
      Painter.Draw(e.Surface.Canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }

    private static readonly System.Reflection.ConstructorInfo painterCtor;
    private static readonly object[] ctorParams;

    #region BindableProperties
    static BaseView() {
      var ctors = typeof(TPainter).GetConstructors().Where(c => c.GetParameters().All(param => param.IsOptional));
      if (ctors.IsEmpty()) throw new ArgumentException($"The supplied generic type parameter for {nameof(TPainter)}, which is {typeof(TPainter)}, does not have any constructors with no parameters nor all optional parameters.");
      painterCtor = ctors.First();
      ctorParams = Enumerable.Repeat(Type.Missing, painterCtor.GetParameters().Length).ToArray();
      var painter = (TPainter)painterCtor.Invoke(ctorParams);
      var thisType = typeof(BaseView<TPainter, TSource>);
      var drawMethodParams = typeof(SkiaSharp.MathPainter).GetMethod(nameof(SkiaSharp.MathPainter.Draw), new[] { typeof(SKCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float) }).GetParameters();
      TPainter p(BindableObject b) => ((BaseView<TPainter, TSource>)b).Painter;
      SourceProperty = BindableProperty.Create(nameof(Source), typeof(TSource), thisType, painter.Source, BindingMode.TwoWay, null, (b, o, n) => { p(b).Source = (TSource)n; ((BaseView<TPainter, TSource>)b).ErrorMessage = p(b).ErrorMessage; });
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
      EnableGesturesProperty = BindableProperty.Create(nameof(EnableGestures), typeof(bool), thisType, false);
      ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, painter.ErrorMessage, BindingMode.OneWayToSource);
      ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
      GestureCountPropertyKey = BindableProperty.CreateReadOnly(nameof(GestureCount), typeof(int), thisType, 0, BindingMode.OneWayToSource);
      GestureCountProperty = GestureCountPropertyKey.BindableProperty;
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
    public static readonly BindableProperty EnableGesturesProperty;
    private static readonly BindablePropertyKey ErrorMessagePropertyKey;
    public static readonly BindableProperty ErrorMessageProperty;
    private static readonly BindablePropertyKey GestureCountPropertyKey;
    public static readonly BindableProperty GestureCountProperty;
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
    protected virtual void OnPinch(object sender, PinchGestureUpdatedEventArgs e) {
      if (Source.IsValid) {
        switch (e.Status) {
          case GestureStatus.Started:
            GestureCount++;
            break;
          case GestureStatus.Running:
            Magnification *= (float)e.Scale;
            InvalidateSurface();
            break;
          case GestureStatus.Completed:
          case GestureStatus.Canceled:
            GestureCount--;
            break;
          default:
            throw new NotImplementedException("A new GestureStatus is in Xamarin.Forms?");
        }
      }
    }

    public bool EnableGestures { get => (bool)GetValue(EnableGesturesProperty); set => SetValue(EnableGesturesProperty, value); }
    public int GestureCount { get => (int)GetValue(GestureCountProperty); private set => SetValue(GestureCountPropertyKey, value); }
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
    public ObservableCollection<Typeface> LocalTypefaces => Painter.LocalTypefaces;

    public new RectangleF? Measure => Painter.Measure;
    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint) => Painter.Measure is RectangleF r ? new SizeRequest(new Xamarin.Forms.Size(r.Width, r.Height)) : base.OnMeasure(widthConstraint, heightConstraint);
  }
}