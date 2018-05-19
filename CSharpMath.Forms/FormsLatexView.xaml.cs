using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp.Views.Forms;

namespace CSharpMath.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile), ContentProperty(nameof(LaTeX))]
	public partial class FormsLatexView : SKCanvasView
	{
    public FormsLatexView() => InitializeComponent();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      var painter = new SkiaSharp.SkiaLatexPainter((float)Width, (float)Height, FontSize) {
        BackgroundColor = BackgroundColor.ToSKColor(),
        DisplayErrorInline = DisplayErrorInline,
        ErrorColor = ErrorColor.ToSKColor(),
        ErrorFontSize = ErrorFontSize,
        LaTeX = LaTeX,
        Padding = new SkiaSharp.Thickness((float)Padding.Left, (float)Padding.Top, (float)Padding.Right, (float)Padding.Bottom),
        PaintStyle = PaintStyle,
        TextAlignment = (Enumerations.ColumnAlignment)HorizontalTextAlignment,
        TextColor = TextColor.ToSKColor()
      };
      painter.Draw(e.Surface.Canvas);
      base.OnPaintSurface(e); 
    }
    
    public string ErrorMessage { get; private set; }
    public bool DisplayErrorInline { get; set; } = true;
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    public float FontSize { get; set; } = 20f;
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; } = null;
    public Color TextColor { get; set; } = Color.Black;
    public Color ErrorColor { get; set; } = Color.Red;
    public TextAlignment HorizontalTextAlignment { get; set; } = TextAlignment.Start;
    public global::SkiaSharp.SKPaintStyle PaintStyle { get; set; } = global::SkiaSharp.SKPaintStyle.StrokeAndFill;
    public Thickness Padding { get; set; }
    public string LaTeX { get; set; }
  }
}