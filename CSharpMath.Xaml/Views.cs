using System;
using System.Collections.Generic;
using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using CSharpMath.Structures;
using Typography.OpenFont;
using CSharpMathTextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;
using CSharpMathThickness = CSharpMath.Structures.Thickness;

// X stands for Xaml
#if Avalonia
using XCanvas = CSharpMath.Avalonia.AvaloniaCanvas;
using XCanvasColor = Avalonia.Media.Color;
using XColor = Avalonia.Media.Color;
using XControl = Avalonia.Controls.Control;
using XInheritControl = Avalonia.Controls.Control;
using XProperty = Avalonia.AvaloniaProperty;
namespace CSharpMath.Avalonia {
#elif Forms
using XCanvas = SkiaSharp.SKCanvas;
using XCanvasColor = SkiaSharp.SKColor;
using XColor = Xamarin.Forms.Color;
using XControl = Xamarin.Forms.View;
using XInheritControl = SkiaSharp.Views.Forms.SKCanvasView;
using XProperty = Xamarin.Forms.BindableProperty;
using MathPainter = CSharpMath.SkiaSharp.MathPainter;
using TextPainter = CSharpMath.SkiaSharp.TextPainter;
namespace CSharpMath.Forms {
  [Xamarin.Forms.ContentProperty(nameof(LaTeX))]
#elif HAS_UNO || WINDOWS_UWP
using Windows.UI.Xaml;
using System.Drawing;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using XCanvas = SkiaSharp.SKCanvas;
using XCanvasColor = SkiaSharp.SKColor;
using XColor = Windows.UI.Color;
using XThickness = Windows.UI.Xaml.Thickness;
using XControl = Windows.UI.Xaml.Controls.Control;
using XInheritControl = SkiaSharp.Views.UWP.SKXamlCanvas;
using XProperty = Windows.UI.Xaml.DependencyProperty;
using MathPainter = CSharpMath.SkiaSharp.MathPainter;
using TextPainter = CSharpMath.SkiaSharp.TextPainter;
namespace CSharpMath.UWPUno {
  [global::Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(LaTeX))]
#endif
 public partial class BaseView<TPainter, TContent> : XInheritControl, ICSharpMathAPI<TContent, XColor>
    where TPainter : Painter<XCanvas, TContent, XCanvasColor>, new() where TContent : class {
    public TPainter Painter { get; } = new TPainter();

    protected static readonly TPainter staticPainter = new TPainter();
    public static XProperty CreateProperty<TThis, TValue>(
      string propertyName,
      bool affectsMeasure,
      Func<TPainter, TValue> defaultValueGet,
      Action<TPainter, TValue> propertySet,
      Action<TThis, TValue>? updateOtherProperty = null)
      where TThis : BaseView<TPainter, TContent> {
      var defaultValue = defaultValueGet(staticPainter);
      void PropertyChanged(TThis @this, object? newValue) {
        // We need to support nullable classes and structs, so we cannot forbid null here
        // So this use of the null-forgiving operator should be blamed on non-generic PropertyChanged handlers
        var @new = (TValue)newValue!;
        propertySet(@this.Painter, @new);
        updateOtherProperty?.Invoke(@this, @new);
        if (affectsMeasure) @this.InvalidateMeasure();
        // Redraw immediately! No deferred drawing
#if Avalonia
        @this.InvalidateVisual();
      }
      var prop = XProperty.Register<TThis, TValue>(propertyName, defaultValue);
      global::Avalonia.AvaloniaObjectExtensions.AddClassHandler<TThis>(prop.Changed, (t, e) => PropertyChanged(t, e.NewValue));
      return prop;
    }
    public BaseView() {
      // Respect built-in styles
      Styles.Add(new global::Avalonia.Styling.Style(global::Avalonia.Styling.Selectors.Is<BaseView<TPainter, TContent>>) {
        Setters =
        {
          new global::Avalonia.Styling.Setter(TextColorProperty, new global::Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("SystemBaseHighColor"))
        }
      });
    }
    protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize) =>
      Painter.Measure((float)availableSize.Width) is { } rect
      ? new global::Avalonia.Size(rect.Width, rect.Height)
      : base.MeasureOverride(availableSize);
    struct ReadOnlyProperty<TThis, TValue> where TThis : BaseView<TPainter, TContent> {
      public ReadOnlyProperty(string propertyName,
        Func<TPainter, TValue> getter) {
        Property = XProperty.RegisterDirect<TThis, TValue>(propertyName, b => getter(b.Painter), null, getter(staticPainter));
        _value = getter(staticPainter);
      }
      TValue _value;
      public global::Avalonia.DirectProperty<TThis, TValue> Property;
      public void SetValue(TThis @this, TValue value) => @this.SetAndRaise(Property, ref _value, value);
    }
    static XCanvasColor XColorToXCanvasColor(XColor color) => color;
    static XColor XCanvasColorToXColor(XCanvasColor color) => color;
    global::Avalonia.Point _origin;
    protected override void OnPointerPressed(global::Avalonia.Input.PointerPressedEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        _origin = point.Position;
      }
      base.OnPointerPressed(e);
    }
    protected override void OnPointerMoved(global::Avalonia.Input.PointerEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        var displacement = point.Position - _origin;
        _origin = point.Position;
        DisplacementX += (float)displacement.X;
        DisplacementY += (float)displacement.Y;
      }
      base.OnPointerMoved(e);
    }
    protected override void OnPointerReleased(global::Avalonia.Input.PointerReleasedEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        _origin = point.Position;
      }
      base.OnPointerReleased(e);
    }
    public override void Render(global::Avalonia.Media.DrawingContext context) {
      base.Render(context);
      var canvas = new XCanvas(context, Bounds.Size);
#elif Forms
        @this.InvalidateSurface();
      }
      return XProperty.Create(propertyName, typeof(TValue), typeof(TThis), defaultValue,
        propertyChanged: (b, o, n) => PropertyChanged((TThis)b, n));
    }
    protected override Xamarin.Forms.SizeRequest OnMeasure(double widthConstraint, double heightConstraint) =>
      Painter.Measure((float)widthConstraint) is { } rect
      ? new Xamarin.Forms.SizeRequest(new Xamarin.Forms.Size(rect.Width, rect.Height))
      : base.OnMeasure(widthConstraint, heightConstraint);
    struct ReadOnlyProperty<TThis, TValue> where TThis : BaseView<TPainter, TContent> {
      public ReadOnlyProperty(string propertyName,
        Func<TPainter, TValue> getter) {
        _key = XProperty.CreateReadOnly(propertyName, typeof(TValue), typeof(TThis), getter(staticPainter));
      }
      readonly Xamarin.Forms.BindablePropertyKey _key;
      public XProperty Property => _key.BindableProperty;
      public void SetValue(TThis @this, TValue value) => @this.SetValue(_key, value);
    }
    private protected static XCanvasColor XColorToXCanvasColor(XColor color) => global::SkiaSharp.Views.Forms.Extensions.ToSKColor(color);
    private protected static XColor XCanvasColorToXColor(XCanvasColor color) => global::SkiaSharp.Views.Forms.Extensions.ToFormsColor(color);
    global::SkiaSharp.SKPoint _origin;
    protected override void OnTouch(global::SkiaSharp.Views.Forms.SKTouchEventArgs e) {
      if (e.InContact && EnablePanning) {
        switch (e.ActionType) {
          case global::SkiaSharp.Views.Forms.SKTouchAction.Pressed:
            _origin = e.Location;
            e.Handled = true;
            break;
          case global::SkiaSharp.Views.Forms.SKTouchAction.Moved:
            var displacement = e.Location - _origin;
            _origin = e.Location;
            DisplacementX += displacement.X;
            DisplacementY += displacement.Y;
            e.Handled = true;
            break;
          case global::SkiaSharp.Views.Forms.SKTouchAction.Released:
            _origin = e.Location;
            e.Handled = true;
            break;
          case global::SkiaSharp.Views.Forms.SKTouchAction.Entered:
          case global::SkiaSharp.Views.Forms.SKTouchAction.Cancelled:
          case global::SkiaSharp.Views.Forms.SKTouchAction.Exited:
          default:
            break;
        }
      }
      base.OnTouch(e);
    }
    protected sealed override void OnPaintSurface(global::SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      var canvas = e.Surface.Canvas;
      canvas.Clear();
      // https://github.com/verybadcat/CSharpMath/issues/136 and https://github.com/verybadcat/CSharpMath/issues/137
      // SkiaSharp deals with raw pixels as opposed to Xamarin.Forms's device-independent units.
      // We should scale to occupy the full view size.
      canvas.Scale(e.Info.Width / (float)Width);
#elif HAS_UNO || WINDOWS_UWP
        @this.Invalidate();
      }
      return XProperty.Register(propertyName, typeof(TValue), typeof(TThis),
        new PropertyMetadata(defaultValue, (b, n) => PropertyChanged((TThis)b, n.NewValue)));
    }
    public BaseView() {
      PointerPressed += OnPointerPressed;
      PointerMoved += OnPointerMoved;
      PointerReleased += OnPointerReleased;
    }
    protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize) =>
      Painter.Measure((float)availableSize.Width) is { } rect
      ? new Windows.Foundation.Size(rect.Width, rect.Height)
      : base.MeasureOverride(availableSize);
    struct ReadOnlyProperty<TThis, TValue> where TThis : BaseView<TPainter, TContent> {
      public ReadOnlyProperty(string propertyName,
        Func<TPainter, TValue> getter) {
        Property = XProperty.Register(propertyName, typeof(TValue), typeof(TThis), new Windows.UI.Xaml.PropertyMetadata(getter(staticPainter)));        
      }
      public XProperty Property;
      public void SetValue(TThis @this, TValue value) => @this.SetValue(Property, value);
    }
    static XCanvasColor XColorToXCanvasColor(XColor color) => new XCanvasColor(color.R, color.G, color.B, color.A);
    static XColor XCanvasColorToXColor(XCanvasColor color) => XColor.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    static CSharpMathThickness XThicknessToCSharpMathThickness(XThickness thickness) => new CSharpMathThickness((float)thickness.Left, (float)thickness.Top, (float)thickness.Right, (float)thickness.Bottom);
    static XThickness CSharpMathThicknessToXThickness(CSharpMathThickness thickness) => new XThickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    global::Windows.Foundation.Point _origin;
    private void OnPointerPressed(object sender, PointerRoutedEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        _origin = point.Position;
      }
    }
    private void OnPointerMoved(object sender, PointerRoutedEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        var displacement = new Windows.Foundation.Point(point.Position.X - _origin.X, point.Position.Y - _origin.Y);
        _origin = point.Position;
        DisplacementX += (float)displacement.X;
        DisplacementY += (float)displacement.Y;
      }
    }
    private void OnPointerReleased(object sender, PointerRoutedEventArgs e) {
      var point = e.GetCurrentPoint(this);
      if (point.Properties.IsLeftButtonPressed && EnablePanning) {
        _origin = point.Position;
      }
    }
    protected override void OnPaintSurface(global::SkiaSharp.Views.UWP.SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      var canvas = e.Surface.Canvas;
      canvas.Clear();
      // SkiaSharp deals with raw pixels as opposed to Xamarin.Forms's device-independent units.
      // We should scale to occupy the full view size.
      canvas.Scale(e.Info.Width / (float)ActualWidth);
#endif
      Painter.Draw(canvas, TextAlignment, Padding, (float)DisplacementX, (float)DisplacementY);
    }
    /// <summary>Requires touch events to be enabled in SkiaSharp/Xamarin.Forms</summary>
    public bool EnablePanning { get => (bool)GetValue(DisablePanningProperty); set => SetValue(DisablePanningProperty, value); }
    public static readonly XProperty DisablePanningProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(EnablePanning), false, _ => false, (_, __) => { });

    static readonly System.Reflection.ParameterInfo[] drawMethodParams = typeof(TPainter)
      .GetMethod(nameof(Painter<XCanvas, TContent, XColor>.Draw),
        new[] { typeof(XCanvas), typeof(CSharpMathTextAlignment), typeof(CSharpMathThickness), typeof(float), typeof(float) }).GetParameters();
    static T? Nullable<T>(T value) where T : struct => new T?(value);
    public (XColor glyph, XColor textRun)? GlyphBoxColor { get => ((XColor glyph, XColor textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public static readonly XProperty GlyphBoxColorProperty = CreateProperty<BaseView<TPainter, TContent>, (XColor glyph, XColor textRun)?>(nameof(GlyphBoxColor), false,
      p => p.GlyphBoxColor is var (glyph, textRun) ? Nullable((XCanvasColorToXColor(glyph), XCanvasColorToXColor(textRun))) : null,
      (p, v) => p.GlyphBoxColor = v is var (glyph, textRun) ? Nullable((XColorToXCanvasColor(glyph), XColorToXCanvasColor(textRun))) : null);
    public TContent? Content { get => (TContent)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly XProperty ContentProperty = CreateProperty<BaseView<TPainter, TContent>, TContent?>(nameof(Content), true, p => p.Content, (p, v) => p.Content = v, (b, v) => { if (b.Painter.ErrorMessage == null) b.LaTeX = b.Painter.LaTeX; });
#if Avalonia
    [global::Avalonia.Metadata.Content]
#endif
    public string? LaTeX { get => (string?)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    public static readonly XProperty LaTeXProperty = CreateProperty<BaseView<TPainter, TContent>, string?>(nameof(LaTeX), true, p => p.LaTeX, (p, v) => p.LaTeX = v, (b, v) => (b.Content, b.ErrorMessage) = (b.Painter.Content, b.Painter.ErrorMessage));
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty); set => SetValue(DisplayErrorInlineProperty, value); }
    public static readonly XProperty DisplayErrorInlineProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(DisplayErrorInline), true, p => p.DisplayErrorInline, (p, v) => p.DisplayErrorInline = v);
    /// <summary>Unit of measure: points</summary>
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public static readonly XProperty FontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, double>(nameof(FontSize), true, p => p.FontSize, (p, v) => p.FontSize = (float)v);
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public double ErrorFontSize { get => (double)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public static readonly XProperty ErrorFontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, double>(nameof(ErrorFontSize), true, p => p.ErrorFontSize, (p, v) => p.ErrorFontSize = (float)v);
    public IEnumerable<Typeface> LocalTypefaces { get => (IEnumerable<Typeface>)GetValue(LocalTypefacesProperty); set => SetValue(LocalTypefacesProperty, value); }
    public static readonly XProperty LocalTypefacesProperty = CreateProperty<BaseView<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces), true, p => p.LocalTypefaces, (p, v) => p.LocalTypefaces = v);
    public XColor TextColor { get => (XColor)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public static readonly XProperty TextColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(TextColor), false, p => XCanvasColorToXColor(p.TextColor), (p, v) => p.TextColor = XColorToXCanvasColor(v));
    public XColor HighlightColor { get => (XColor)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public static readonly XProperty HighlightColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(HighlightColor), false, p => XCanvasColorToXColor(p.HighlightColor), (p, v) => p.HighlightColor = XColorToXCanvasColor(v));
    public XColor ErrorColor { get => (XColor)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public static readonly XProperty ErrorColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(ErrorColor), false, p => XCanvasColorToXColor(p.ErrorColor), (p, v) => p.ErrorColor = XColorToXCanvasColor(v));
    public CSharpMathTextAlignment TextAlignment { get => (CSharpMathTextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public static readonly XProperty TextAlignmentProperty = CreateProperty<BaseView<TPainter, TContent>, CSharpMathTextAlignment>(nameof(Rendering.FrontEnd.TextAlignment), false, p => (CSharpMathTextAlignment)drawMethodParams[1].DefaultValue, (p, v) => { });
    public CSharpMathThickness Padding { get => (CSharpMathThickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public static readonly XProperty PaddingProperty = CreateProperty<BaseView<TPainter, TContent>, CSharpMathThickness>(nameof(Padding), false, p => (CSharpMathThickness)(drawMethodParams[2].DefaultValue ?? new CSharpMathThickness()), (p, v) => { });
    public double DisplacementX { get => (double)GetValue(DisplacementXProperty); set => SetValue(DisplacementXProperty, value); }
    public static readonly XProperty DisplacementXProperty = CreateProperty<BaseView<TPainter, TContent>, double>(nameof(DisplacementX), false, p => Convert.ToDouble(drawMethodParams[3].DefaultValue), (p, v) => { });
    public double DisplacementY { get => (double)GetValue(DisplacementYProperty); set => SetValue(DisplacementYProperty, value); }
    public static readonly XProperty DisplacementYProperty = CreateProperty<BaseView<TPainter, TContent>, double>(nameof(DisplacementY), false, p => Convert.ToDouble(drawMethodParams[4].DefaultValue), (p, v) => { });
    public double Magnification { get => (double)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public static readonly XProperty MagnificationProperty = CreateProperty<BaseView<TPainter, TContent>, double>(nameof(Magnification), false, p => p.Magnification, (p, v) => p.Magnification = (float)v);
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public static readonly XProperty PaintStyleProperty = CreateProperty<BaseView<TPainter, TContent>, PaintStyle>(nameof(PaintStyle), false, p => p.PaintStyle, (p, v) => p.PaintStyle = v);
    public LineStyle LineStyle { get => (LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public static readonly XProperty LineStyleProperty = CreateProperty<BaseView<TPainter, TContent>, LineStyle>(nameof(LineStyle), false, p => p.LineStyle, (p, v) => p.LineStyle = v);
    public string? ErrorMessage { get => (string?)GetValue(ErrorMessageProperty); private set => ErrorMessagePropertyKey.SetValue(this, value); }
    private static readonly ReadOnlyProperty<BaseView<TPainter, TContent>, string?> ErrorMessagePropertyKey = new ReadOnlyProperty<BaseView<TPainter, TContent>, string?>(nameof(ErrorMessage), p => p.ErrorMessage);
    public static readonly XProperty ErrorMessageProperty = ErrorMessagePropertyKey.Property;
  }
  public partial class MathView : BaseView<MathPainter, MathList> { }
  public partial class TextView : BaseView<TextPainter, Rendering.Text.TextAtom> { }
}
