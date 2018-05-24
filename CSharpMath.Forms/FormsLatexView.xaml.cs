using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms {
  [XamlCompilation(XamlCompilationOptions.Compile), ContentProperty(nameof(LaTeX))]
  public partial class FormsLatexView : SKCanvasView {
    public FormsLatexView() {
      InitializeComponent();
      painter = new SkiaSharp.SkiaLatexPainter(InvalidateSurface, CanvasSize);
    }

    protected SkiaSharp.SkiaLatexPainter painter;
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      painter.Bounds = CanvasSize;
      painter.Draw(e.Surface.Canvas);
      base.OnPaintSurface(e);
    }
    #region BindableProperties
    private static readonly Type thisType = typeof(FormsLatexView);
    public static readonly BindableProperty LaTeXProperty = BindableProperty.Create(nameof(LaTeX), typeof(string), thisType, null, BindingMode.TwoWay);
    public static readonly BindableProperty DisplayErrorInlineProperty = BindableProperty.Create(nameof(DisplayErrorInline), typeof(bool), thisType, true, BindingMode.OneWay);
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(float), thisType);
    public static readonly BindableProperty ErrorFontSizeProperty = BindableProperty.Create(nameof(ErrorFontSize), typeof(float?), thisType);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), thisType);
    public static readonly BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), thisType);
    public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), thisType);
    public static readonly BindableProperty PaintStyleProperty = BindableProperty.Create(nameof(PaintStyle), typeof(global::SkiaSharp.SKPaintStyle), thisType);
    public static readonly BindableProperty DrawGlyphBoxesProperty = BindableProperty.Create(nameof(DrawGlyphBoxes), typeof(bool), thisType);
    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), thisType);
    public static readonly BindableProperty MathListProperty = BindableProperty.Create(nameof(MathList), typeof(Interfaces.IMathList), thisType, null, BindingMode.TwoWay);
    internal static readonly BindablePropertyKey ErrorMessagePropertyKey = BindableProperty.CreateReadOnly(nameof(ErrorMessage), typeof(string), thisType, null, BindingMode.OneWayToSource);
    public static readonly BindableProperty ErrorMessageProperty = ErrorMessagePropertyKey.BindableProperty;
    #endregion

    bool LaTeX_MathList_Lock = false;
    protected override void OnPropertyChanged(string propertyName) {
      base.OnPropertyChanged(propertyName);    // Be sure to do all the "normal" activities of the base class

      switch (propertyName) {
        case nameof(LaTeX):
          painter.LaTeX = LaTeX;
          if (!LaTeX_MathList_Lock) {
            SetValue(ErrorMessagePropertyKey, painter.ErrorMessage);
            SetValue(MathListProperty, painter.MathList);
            LaTeX_MathList_Lock = true;
          } else LaTeX_MathList_Lock = false;
          break;
        case nameof(DisplayErrorInline): painter.DisplayErrorInline = DisplayErrorInline; break;
        case nameof(FontSize): painter.FontSize = FontSize; break;
        case nameof(ErrorFontSize): painter.ErrorFontSize = ErrorFontSize; break;
        case nameof(TextColor): painter.TextColor = TextColor.ToSKColor(); break;
        case nameof(ErrorColor): painter.ErrorColor = ErrorColor.ToSKColor(); break;
        case nameof(HorizontalTextAlignment): painter.TextAlignment = (Enumerations.ColumnAlignment)HorizontalTextAlignment; break;
        case nameof(PaintStyle): painter.PaintStyle = PaintStyle; break;
        case nameof(DrawGlyphBoxes): painter.DrawGlyphBoxes = DrawGlyphBoxes; break;
        case nameof(Padding): painter.Padding = new SkiaSharp.Thickness((float)Padding.Left, (float)Padding.Top, (float)Padding.Right, (float)Padding.Bottom); break;
        case nameof(MathList):
          painter.MathList = MathList;
          if (!LaTeX_MathList_Lock) {
            SetValue(LaTeXProperty, painter.LaTeX);
            LaTeX_MathList_Lock = true;
          } else LaTeX_MathList_Lock = false;
          break;
        case nameof(ErrorMessage): break; //Only can be set from this class
      }
    }

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
    public TextAlignment HorizontalTextAlignment { get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty); set => SetValue(HorizontalTextAlignmentProperty, value); }
    public global::SkiaSharp.SKPaintStyle PaintStyle { get => (global::SkiaSharp.SKPaintStyle)GetValue(PaintStyleProperty); set => SetValue(PaintStyleProperty, value); }
    public bool DrawGlyphBoxes { get => (bool)GetValue(DrawGlyphBoxesProperty); set => SetValue(DrawGlyphBoxesProperty, value); }
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }
    public Interfaces.IMathList MathList { get => (Interfaces.IMathList)GetValue(MathListProperty); set => SetValue(MathListProperty, value); }
    public string ErrorMessage => (string)GetValue(ErrorMessageProperty);
  }
}