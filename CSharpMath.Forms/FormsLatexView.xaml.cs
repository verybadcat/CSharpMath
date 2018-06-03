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
      LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, painter.LaTeX, BindingMode.TwoWay);
      DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, painter.DisplayErrorInline, BindingMode.OneWay);
      FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType, painter.FontSize);
      ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType, painter.ErrorFontSize);
      TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType, painter.TextColor.ToFormsColor());
      ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType, painter.ErrorColor.ToFormsColor());
      TextAlignmentProperty = BindableProperty.Create(nameof(TextAlignment), typeof(SkiaSharp.SkiaTextAlignment), thisType, painter.TextAlignment);
      OriginXProperty = BindableProperty.Create(nameof(OriginX), typeof(float?), thisType, painter.OriginX, BindingMode.TwoWay);
      OriginYProperty = BindableProperty.Create(nameof(OriginY), typeof(float?), thisType, painter.OriginY, BindingMode.TwoWay);
      MagnificationProperty = BindableProperty.Create(nameof(Magnification), typeof(float), thisType, painter.Magnification);
      PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(SKStyle), thisType, painter.PaintStyle);
      LineStyleProperty = BindableProperty.Create(nameof(LineStyle), typeof(Enumerations.LineStyle), thisType, painter.LineStyle);
      DrawGlyphBoxesProperty = BindableProperty.Create(nameof(DrawGlyphBoxes), typeof(bool), thisType, painter.DrawGlyphBoxes);
      PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType, new Thickness(painter.Padding.Left, painter.Padding.Top, painter.Padding.Right, painter.Padding.Bottom));
      MathListProperty = BindableProperty.Create(nameof(MathList), typeof(Interfaces.IMathList), thisType, painter.MathList, BindingMode.TwoWay);
      LockGesturesProperty = BindableProperty.Create(nameof(LockGestures), typeof(bool), thisType, false);
      ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, painter.ErrorMessage, BindingMode.OneWayToSource);
      ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
      GestureCountPropertyKey = BindableProperty.CreateReadOnly(nameof(GestureCount), typeof(int), thisType, 0, BindingMode.OneWayToSource);
      GestureCountProperty = GestureCountPropertyKey.BindableProperty;
    }
    public static readonly BindableProperty LaTeXProperty;
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
    public static readonly BindableProperty DrawGlyphBoxesProperty;
    public static readonly BindableProperty PaddingProperty;
    public static readonly BindableProperty MathListProperty;
    public static readonly BindableProperty LockGesturesProperty;
    private static readonly BindablePropertyKey ErrorMessagePropertyKey;
    public static readonly BindableProperty ErrorMessageProperty;
    private static readonly BindablePropertyKey GestureCountPropertyKey;
    public static readonly BindableProperty GestureCountProperty;
    #endregion

    bool _LaTeX_MathList_Lock = false;
    protected override void OnPropertyChanged(string propertyName) {
      base.OnPropertyChanged(propertyName); //Be sure to do all the "normal" activities of the base class

      switch (propertyName) {
        case nameof(LaTeX):
          painter.LaTeX = LaTeX;
          if (!_LaTeX_MathList_Lock) {
            SetValue(ErrorMessagePropertyKey, painter.ErrorMessage);
            SetValue(MathListProperty, painter.MathList);
            _LaTeX_MathList_Lock = true;
          } else _LaTeX_MathList_Lock = false;
          break;
        case nameof(DisplayErrorInline): painter.DisplayErrorInline = DisplayErrorInline; break;
        case nameof(FontSize): painter.FontSize = FontSize; break;
        case nameof(ErrorFontSize): painter.ErrorFontSize = ErrorFontSize; break;
        case nameof(TextColor): painter.TextColor = TextColor.ToSKColor(); break;
        case nameof(ErrorColor): painter.ErrorColor = ErrorColor.ToSKColor(); break;
        case nameof(TextAlignment): painter.TextAlignment = TextAlignment; break;
        case nameof(OriginX): if (painter.OriginX != OriginX) painter.OriginX = OriginX; break;
        case nameof(OriginY): if (painter.OriginY != OriginY) painter.OriginY = OriginY; break;
        case nameof(Magnification): painter.Magnification = Magnification; break;
        case nameof(PaintStyle): painter.PaintStyle = PaintStyle; break;
        case nameof(LineStyle): painter.LineStyle = LineStyle; break;
        case nameof(DrawGlyphBoxes): painter.DrawGlyphBoxes = DrawGlyphBoxes; break;
        case nameof(Padding): painter.Padding = new SkiaSharp.Thickness((float)Padding.Left, (float)Padding.Top, (float)Padding.Right, (float)Padding.Bottom); break;
        case nameof(MathList):
          painter.MathList = MathList;
          if (!_LaTeX_MathList_Lock) {
            SetValue(LaTeXProperty, painter.LaTeX);
            _LaTeX_MathList_Lock = true;
          } else _LaTeX_MathList_Lock = false;
          break;
        case nameof(LockGestures):
          GestureCount = 0;
          break;
        case nameof(ErrorMessage):
        case nameof(GestureCount):
          break; //Only can be set from this class
      }
    }

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
    public string LaTeX { get => (string)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
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
    public SkiaSharp.SkiaTextAlignment TextAlignment { get => (SkiaSharp.SkiaTextAlignment)GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }
    public float? OriginX { get => (float?)GetValue(OriginXProperty); set => SetValue(OriginXProperty, value); }
    public float? OriginY { get => (float?)GetValue(OriginYProperty); set => SetValue(OriginYProperty, value); }
    public float Magnification { get => (float)GetValue(MagnificationProperty); set => SetValue(MagnificationProperty, value); }
    public SKStyle PaintStyle { get => (SKStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public Enumerations.LineStyle LineStyle { get => (Enumerations.LineStyle)GetValue(LineStyleProperty); set => SetValue(LineStyleProperty, value); }
    public bool DrawGlyphBoxes { get => (bool)GetValue(DrawGlyphBoxesProperty); set => SetValue(DrawGlyphBoxesProperty, value); }
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public Interfaces.IMathList MathList { get => (Interfaces.IMathList)GetValue(MathListProperty); set => SetValue(MathListProperty, value); }
    public string ErrorMessage => (string)GetValue(ErrorMessageProperty);
  }
}