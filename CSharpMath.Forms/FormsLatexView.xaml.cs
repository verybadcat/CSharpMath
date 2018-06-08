using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using SKStyle = SkiaSharp.SKPaintStyle;

namespace CSharpMath.Forms {
  using MathSource = Rendering.MathSource;
  [XamlCompilation(XamlCompilationOptions.Compile), ContentProperty(nameof(LaTeX))]
  public partial class FormsLatexView : SKCanvasView {
    public FormsLatexView() {
      InitializeComponent();
      painter = new SkiaSharp.SkiaLatexPainter(CanvasSize);
      var pan = new PanGestureRecognizer { TouchPoints = 1 };
      pan.PanUpdated += OnPan;
      GestureRecognizers.Add(pan);
      var pinch = new PinchGestureRecognizer();
      pinch.PinchUpdated += OnPinch;
      GestureRecognizers.Add(pinch);
    }

    protected SkiaSharp.SkiaLatexPainter painter;
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      painter.Bounds = CanvasSize;
      painter.Draw(e.Surface.Canvas);
      OriginX = painter.OriginX;
      OriginY = painter.OriginY;
    }

    protected override void OnSizeAllocated(double width, double height) {
      base.OnSizeAllocated(width, height);
      OriginX = OriginY = null; //Reset location of origin when view size is changed
    }

    #region BindableProperties
    static FormsLatexView() {
      var painter = new SkiaSharp.SkiaLatexPainter(default);
      var thisType = typeof(FormsLatexView);
      SkiaSharp.SkiaLatexPainter p(BindableObject b) => ((FormsLatexView)b).painter;
      SourceProperty = BindableProperty.Create(nameof(Source), typeof(MathSource), thisType, painter.Source, BindingMode.TwoWay, null, (b, o, n) => { p(b).Source = (MathSource)n; ((FormsLatexView)b).ErrorMessage = p(b).ErrorMessage; });
      DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, painter.DisplayErrorInline, propertyChanged: (b, o, n) => p(b).DisplayErrorInline = (bool)n);
      FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType, painter.FontSize, propertyChanged: (b, o, n) => p(b).FontSize = (float)n);
      ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType, painter.ErrorFontSize, propertyChanged: (b, o, n) => p(b).ErrorFontSize = (float)n);
      TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType, painter.TextColor.ToFormsColor(), propertyChanged: (b, o, n) => p(b).TextColor = ((Color)n).ToSKColor());
      ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType, painter.ErrorColor.ToFormsColor(), propertyChanged: (b, o, n) => p(b).ErrorColor = ((Color)n).ToSKColor());
      TextAlignmentProperty = BindableProperty.Create(nameof(TextAlignment), typeof(Rendering.TextAlignment), thisType, painter.TextAlignment, propertyChanged: (b, o, n) => p(b).TextAlignment = (Rendering.TextAlignment)n);
      OriginXProperty = BindableProperty.Create(nameof(OriginX), typeof(float?), thisType, painter.OriginX, BindingMode.TwoWay, propertyChanged: (b, o, n) => p(b).OriginX = (float?)n);
      OriginYProperty = BindableProperty.Create(nameof(OriginY), typeof(float?), thisType, painter.OriginY, BindingMode.TwoWay, propertyChanged: (b, o, n) => p(b).OriginY = (float?)n);
      MagnificationProperty = BindableProperty.Create(nameof(Magnification), typeof(float), thisType, painter.Magnification, propertyChanged: (b, o, n) => p(b).Magnification = (float)n);
      PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(SKStyle), thisType, painter.PaintStyle, propertyChanged: (b, o, n) => p(b).PaintStyle = (SKStyle)n);
      LineStyleProperty = BindableProperty.Create(nameof(LineStyle), typeof(Enumerations.LineStyle), thisType, painter.LineStyle, propertyChanged: (b, o, n) => p(b).LineStyle = (Enumerations.LineStyle)n);
      GlyphBoxColorProperty = BindableProperty.Create(nameof(GlyphBoxColor), typeof((Color glyph, Color textRun)?), thisType, painter.GlyphBoxColor.HasValue ? (painter.GlyphBoxColor.Value.glyph.ToNative(), painter.GlyphBoxColor.Value.textRun.ToNative()) : default((Color glyph, Color textRun)?), propertyChanged: (b, o, n) => p(b).GlyphBoxColor = n != null ? ((((Color glyph, Color textRun)?)n).Value.glyph.FromNative(), (((Color glyph, Color textRun)?)n).Value.textRun.FromNative()) : default((Structures.Color glyph, Structures.Color textRun)?));
      PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType, new Thickness(painter.Padding.Left, painter.Padding.Top, painter.Padding.Right, painter.Padding.Bottom), propertyChanged: (b, o, n) => p(b).Padding = new SkiaSharp.Thickness((float)((Thickness)n).Left, (float)((Thickness)n).Top, (float)((Thickness)n).Right, (float)((Thickness)n).Bottom));
      LockGesturesProperty = BindableProperty.Create(nameof(LockGestures), typeof(bool), thisType, false);
      ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, painter.ErrorMessage, BindingMode.OneWayToSource);
      ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
      GestureCountPropertyKey = BindableProperty.CreateReadOnly(nameof(GestureCount), typeof(int), thisType, 0, BindingMode.OneWayToSource);
      GestureCountProperty = GestureCountPropertyKey.BindableProperty;
    }
    public static readonly BindableProperty DisplayErrorInlineProperty;
    public static readonly BindableProperty FontSizeProperty;
    public static readonly BindableProperty ErrorFontSizeProperty;
    public static readonly BindableProperty TextColorProperty;
    public static readonly BindableProperty ErrorColorProperty;
    public static readonly BindableProperty TextAlignmentProperty;
    public static readonly BindableProperty OriginXProperty;
    public static readonly BindableProperty OriginYProperty;
    public static readonly BindableProperty MagnificationProperty;
    public static readonly BindableProperty PaintStyleProperty;
    public static readonly BindableProperty LineStyleProperty;
    public static readonly BindableProperty GlyphBoxColorProperty;
    public static readonly BindableProperty PaddingProperty;
    public static readonly BindableProperty SourceProperty;
    public static readonly BindableProperty LockGesturesProperty;
    private static readonly BindablePropertyKey ErrorMessagePropertyKey;
    public static readonly BindableProperty ErrorMessageProperty;
    private static readonly BindablePropertyKey GestureCountPropertyKey;
    public static readonly BindableProperty GestureCountProperty;
    #endregion

    double _lastX, _lastY;
    protected virtual void OnPan(object sender, PanUpdatedEventArgs e) {
      if (!LockGestures && painter.LaTeX.IsNonEmpty()) {
        switch (e.StatusType) {
          case GestureStatus.Started:
            GestureCount++;
            _lastX = _lastY = 0;
            break;
          case GestureStatus.Running:
            OriginX += (float)(e.TotalX - _lastX) / Magnification;
            OriginY += (float)-(e.TotalY - _lastY) / Magnification;
            InvalidateSurface();
            _lastX = e.TotalX;
            _lastY = e.TotalY;
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
    protected virtual void OnPinch(object sender, PinchGestureUpdatedEventArgs e) {
      if (!LockGestures && painter.LaTeX.IsNonEmpty()) {
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

    public bool LockGestures { get => (bool)GetValue(LockGesturesProperty); set => SetValue(LockGesturesProperty, value); }
    public int GestureCount { get => (int)GetValue(GestureCountProperty); private set => SetValue(GestureCountPropertyKey, value); }
    public MathSource Source { get => (MathSource)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public Interfaces.IMathList MathList { get => Source.MathList; set => Source = new MathSource(value); }
    public string LaTeX { get => Source.LaTeX; set => Source = new MathSource(value); }
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
    public Color ErrorColor { get => (Color)GetValue(ErrorColorProperty); set => SetValue(ErrorColorProperty, value); }
    public Rendering.TextAlignment TextAlignment { get => (Rendering.TextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public float? OriginX { get => (float?)GetValue(OriginXProperty); set => SetValue(OriginXProperty, value); }
    public float? OriginY { get => (float?)GetValue(OriginYProperty); set => SetValue(OriginYProperty, value); }
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public SKStyle PaintStyle { get => (SKStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public Enumerations.LineStyle LineStyle { get => (Enumerations.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public (Color glyph, Color textRun)? GlyphBoxColor { get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }
  }
}