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

    public string ErrorMessage => painter.ErrorMessage;
    public bool DisplayErrorInline { get => painter.DisplayErrorInline; set => painter.DisplayErrorInline = value; }
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get => painter.FontSize; set => painter.FontSize = value; }
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get => painter.ErrorFontSize; set => painter.ErrorFontSize = value; }
    public Color TextColor { get => painter.TextColor.ToFormsColor(); set => painter.TextColor = value.ToSKColor(); }
    public Color ErrorColor { get => painter.ErrorColor.ToFormsColor(); set => painter.ErrorColor = value.ToSKColor(); }
    public TextAlignment HorizontalTextAlignment { get => (TextAlignment)painter.TextAlignment; set => painter.TextAlignment = (Enumerations.ColumnAlignment)value; }
    public global::SkiaSharp.SKPaintStyle PaintStyle { get => painter.PaintStyle; set => painter.PaintStyle = value; }
    public Thickness Padding {
      get => new Thickness(painter.Padding.Left, painter.Padding.Top, painter.Padding.Right, painter.Padding.Bottom);
      set => painter.Padding = new SkiaSharp.Thickness((float)Padding.Left, (float)Padding.Top, (float)Padding.Right, (float)Padding.Bottom);
    }
    public string LaTeX { get => painter.LaTeX; set => painter.LaTeX = value; }
    public Interfaces.IMathList MathList { get => painter.MathList; set => painter.MathList = value; }
  }
}