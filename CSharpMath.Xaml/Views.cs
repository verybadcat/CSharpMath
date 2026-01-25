using System;
using System.Collections.Generic;
using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using Typography.OpenFont;
using System.Drawing;
using System.Linq;

// X stands for Xaml
#if Avalonia
using XCanvas = CSharpMath.Avalonia.AvaloniaCanvas;
using XCanvasColor = Avalonia.Media.Color;
using XColor = Avalonia.Media.Color;
using XThickness = Avalonia.Thickness;
using XInheritControl = Avalonia.Controls.Control;
using XProperty = Avalonia.AvaloniaProperty;
namespace CSharpMath.Avalonia {
#elif Maui
using XCanvas_Canvas = Microsoft.Maui.Graphics.ICanvas;
using XCanvas = (Microsoft.Maui.Graphics.ICanvas, Microsoft.Maui.Graphics.SizeF);
using XCanvasColor = Microsoft.Maui.Graphics.Color;
using XColor = Microsoft.Maui.Graphics.Color;
using XThickness = Microsoft.Maui.Thickness;
using XInheritControl = Microsoft.Maui.Controls.GraphicsView;
using XProperty = Microsoft.Maui.Controls.BindableProperty;
namespace CSharpMath.Maui {
  [Microsoft.Maui.Controls.ContentProperty(nameof(LaTeX))]
#endif
  public class BaseView<TPainter, TContent> : XInheritControl, ICSharpMathAPI<TContent, XColor>
    where TPainter : Painter<XCanvas, TContent, XCanvasColor>, new() where TContent : class {
    public TPainter Painter { get; } = new TPainter();

    protected static readonly TPainter staticPainter = new TPainter();

    /// <summary>Contains all the properties to listen to for painter property changes. Do not mutate.</summary>
    public static readonly XProperty[] WritablePainterProperties;
    /// <summary>Contains all the property names to listen to for painter property changes. Do not mutate.</summary>
    public static readonly HashSet<string> WritablePainterPropertyNames;
    static BaseView() {
      WritablePainterProperties = [
        EnablePanningProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(EnablePanning), false, _ => false, (_, __) => { }),
        GlyphBoxColorProperty = CreateProperty<BaseView<TPainter, TContent>, (XColor glyph, XColor textRun)?>(nameof(GlyphBoxColor), false,
          p => p.GlyphBoxColor is var (glyph, textRun) ? Nullable((XCanvasColorToXColor(glyph), XCanvasColorToXColor(textRun))) : null,
          (p, v) => p.GlyphBoxColor = v is var (glyph, textRun) ? Nullable((XColorToXCanvasColor(glyph), XColorToXCanvasColor(textRun))) : null),
        ContentProperty = CreateProperty<BaseView<TPainter, TContent>, TContent?>(nameof(Content), true, p => p.Content, (p, v) => p.Content = v, (b, v) => { if (b.Painter.ErrorMessage == null) b.LaTeX = b.Painter.LaTeX; }),
        LaTeXProperty = CreateProperty<BaseView<TPainter, TContent>, string?>(nameof(LaTeX), true, p => p.LaTeX, (p, v) => p.LaTeX = v, (b, v) => (b.Content, b.ErrorMessage) = (b.Painter.Content, b.Painter.ErrorMessage)),
        DisplayErrorInlineProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(DisplayErrorInline), true, p => p.DisplayErrorInline, (p, v) => p.DisplayErrorInline = v),
        FontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(FontSize), true, p => p.FontSize, (p, v) => p.FontSize = v),
        ErrorFontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, float?>(nameof(ErrorFontSize), true, p => p.ErrorFontSize, (p, v) => p.ErrorFontSize = v),
        LocalTypefacesProperty = CreateProperty<BaseView<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces), true, p => p.LocalTypefaces, (p, v) => p.LocalTypefaces = v),
        TextColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(TextColor), false, p => XCanvasColorToXColor(p.TextColor), (p, v) => p.TextColor = XColorToXCanvasColor(v)),
        HighlightColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(HighlightColor), false, p => XCanvasColorToXColor(p.HighlightColor), (p, v) => p.HighlightColor = XColorToXCanvasColor(v)),
        ErrorColorProperty = CreateProperty<BaseView<TPainter, TContent>, XColor>(nameof(ErrorColor), false, p => XCanvasColorToXColor(p.ErrorColor), (p, v) => p.ErrorColor = XColorToXCanvasColor(v)),
        TextAlignmentProperty = CreateProperty<BaseView<TPainter, TContent>, TextAlignment>(nameof(Rendering.FrontEnd.TextAlignment), false, p => (TextAlignment)drawMethodParams[1].DefaultValue!, (p, v) => { }),
        PaddingProperty = CreateProperty<BaseView<TPainter, TContent>, XThickness>(nameof(Padding), false, p => drawMethodParams[2].DefaultValue is Thickness t ? new XThickness(left: t.Left, top: t.Top, right: t.Right, bottom: t.Bottom) : new XThickness(), (p, v) => { }),
        DisplacementXProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementX), false, p => (float)drawMethodParams[3].DefaultValue!, (p, v) => { }),
        DisplacementYProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementY), false, p => (float)drawMethodParams[4].DefaultValue!, (p, v) => { }),
        MagnificationProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(Magnification), false, p => p.Magnification, (p, v) => p.Magnification = v),
        PaintStyleProperty = CreateProperty<BaseView<TPainter, TContent>, PaintStyle>(nameof(PaintStyle), false, p => p.PaintStyle, (p, v) => p.PaintStyle = v),
        LineStyleProperty = CreateProperty<BaseView<TPainter, TContent>, LineStyle>(nameof(LineStyle), false, p => p.LineStyle, (p, v) => p.LineStyle = v),
      ];
      WritablePainterPropertyNames = [.. WritablePainterProperties.Select(GetPropertyName)];
      ErrorMessagePropertyKey = new ReadOnlyProperty<BaseView<TPainter, TContent>, string?>(nameof(ErrorMessage), p => p.ErrorMessage);
      ErrorMessageProperty = ErrorMessagePropertyKey.Property;
      static XProperty CreateProperty<TThis, TValue>(
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
      static string GetPropertyName(XProperty prop) => prop.Name;
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
    struct ReadOnlyProperty<TThis, TValue>(string propertyName, Func<TPainter, TValue> getter) where TThis : BaseView<TPainter, TContent> {
      TValue _value = getter(staticPainter);
      public global::Avalonia.DirectProperty<TThis, TValue> Property = XProperty.RegisterDirect<TThis, TValue>(propertyName, b => getter(b.Painter), null, getter(staticPainter));
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
#elif Maui
          @this.Invalidate();
        }
        return XProperty.Create(propertyName, typeof(TValue), typeof(TThis), defaultValue,
          propertyChanged: (b, o, n) => PropertyChanged((TThis)b, n));
      }
      static string GetPropertyName(XProperty prop) => prop.PropertyName;
    }
    protected override Microsoft.Maui.Graphics.Size MeasureOverride(double widthConstraint, double heightConstraint) =>
      Painter.Measure((float)widthConstraint) is { } rect
      ? new(Math.Min(rect.Width, widthConstraint), Math.Min(rect.Height, heightConstraint)) // We can't allocate too big of a GraphicsView on MAUI Windows.
      : base.MeasureOverride(widthConstraint, heightConstraint);
    readonly struct ReadOnlyProperty<TThis, TValue>(string propertyName, Func<TPainter, TValue> getter) where TThis : BaseView<TPainter, TContent> {
      readonly Microsoft.Maui.Controls.BindablePropertyKey _key = XProperty.CreateReadOnly(propertyName, typeof(TValue), typeof(TThis), getter(staticPainter));
      public XProperty Property => _key.BindableProperty;
      public void SetValue(TThis @this, TValue value) => @this.SetValue(_key, value);
    }
    private protected static XCanvasColor XColorToXCanvasColor(XColor color) => color;
    private protected static XColor XCanvasColorToXColor(XCanvasColor color) => color;
    Microsoft.Maui.Graphics.PointF _origin;
    public BaseView() {
      StartInteraction += (sender, e) => {
        if (EnablePanning)
          _origin = e.Touches[0];
      };
      DragInteraction += (sender, e) => {
        if (EnablePanning) {
          var point = e.Touches[0];
          var displacement = point - _origin;
          _origin = point;
          DisplacementX += displacement.Width;
          DisplacementY += displacement.Height;
        }
      };
      EndInteraction += (sender, e) => { _origin = e.Touches[0]; };
      Drawable = new DrawableRedirector(this);
    }
    class DrawableRedirector(BaseView<TPainter, TContent> parent) : Microsoft.Maui.Graphics.IDrawable {
  
      public void Draw(XCanvas_Canvas canvas, Microsoft.Maui.Graphics.RectF dirtyRect) => parent.Draw(canvas);
    }
    void Draw(XCanvas_Canvas rawCanvas) {
      // dirtyRect may be larger than Width and Height on Windows which leads to incorrect alignments
      var canvas = (rawCanvas, new Microsoft.Maui.Graphics.SizeF((float)Width, (float)Height));
#endif
      var padding = Padding;
      Painter.Draw(canvas, TextAlignment, new(left: (float)padding.Left, top: (float)padding.Top, right: (float)padding.Right, bottom: (float)padding.Bottom), DisplacementX, DisplacementY);
    }
    /// <summary>Requires touch events to be enabled in SkiaSharp/Xamarin.Forms</summary>
    public bool EnablePanning { get => (bool)GetValue(EnablePanningProperty)!; set => SetValue(EnablePanningProperty, value); }
    public static readonly XProperty EnablePanningProperty;

    static readonly System.Reflection.ParameterInfo[] drawMethodParams = typeof(TPainter)
      .GetMethod(nameof(Painter<XCanvas, TContent, XColor>.Draw),
        [typeof(XCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float)])!.GetParameters();
    static T? Nullable<T>(T value) where T : struct => new T?(value);
    public (XColor glyph, XColor textRun)? GlyphBoxColor { get => ((XColor glyph, XColor textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public static readonly XProperty GlyphBoxColorProperty;
    public TContent? Content { get => (TContent?)GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
    public static readonly XProperty ContentProperty;
#if Avalonia
    [global::Avalonia.Metadata.Content]
#endif
    public string? LaTeX { get => (string?)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    public static readonly XProperty LaTeXProperty;
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty)!; set => SetValue(DisplayErrorInlineProperty, value); }
    public static readonly XProperty DisplayErrorInlineProperty;
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty)!; set => SetValue(FontSizeProperty, value); }
    public static readonly XProperty FontSizeProperty;
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    public static readonly XProperty ErrorFontSizeProperty;
    public IEnumerable<Typeface> LocalTypefaces { get => (IEnumerable<Typeface>)GetValue(LocalTypefacesProperty)!; set => SetValue(LocalTypefacesProperty, value); }
    public static readonly XProperty LocalTypefacesProperty;
    public XColor TextColor { get => (XColor)GetValue(TextColorProperty)!; set => SetValue(TextColorProperty, value); }
    public static readonly XProperty TextColorProperty;
    public XColor HighlightColor { get => (XColor)GetValue(HighlightColorProperty)!; set => SetValue(HighlightColorProperty, value); }
    public static readonly XProperty HighlightColorProperty;
    public XColor ErrorColor { get => (XColor)GetValue(ErrorColorProperty)!; set => SetValue(ErrorColorProperty, value); }
    public static readonly XProperty ErrorColorProperty;
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty)!; set => SetValue(TextAlignmentProperty, value); }
    public static readonly XProperty TextAlignmentProperty;
    public XThickness Padding { get => (XThickness)GetValue(PaddingProperty)!; set => SetValue(PaddingProperty, value); }
    public static readonly XProperty PaddingProperty;
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty)!; set => SetValue(DisplacementXProperty, value); }
    public static readonly XProperty DisplacementXProperty;
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty)!; set => SetValue(DisplacementYProperty, value); }
    public static readonly XProperty DisplacementYProperty;
    public float Magnification { get => (float)GetValue(MagnificationProperty)!; set => SetValue(MagnificationProperty, value); }
    public static readonly XProperty MagnificationProperty;
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty)!; set => SetValue(PaintStyleProperty, value); }
    public static readonly XProperty PaintStyleProperty;
    public LineStyle LineStyle { get => (LineStyle)GetValue(LineStyleProperty)!; set => SetValue(LineStyleProperty, value); }
    public static readonly XProperty LineStyleProperty;
    public string? ErrorMessage { get => (string?)GetValue(ErrorMessageProperty); private set => ErrorMessagePropertyKey.SetValue(this, value); }
    private static readonly ReadOnlyProperty<BaseView<TPainter, TContent>, string?> ErrorMessagePropertyKey;
    public static readonly XProperty ErrorMessageProperty;
  }
  public class MathView : BaseView<MathPainter, MathList> { }
  public class TextView : BaseView<TextPainter, Rendering.Text.TextAtom> { }
}
