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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FormsLatexView : SKCanvasView
	{
    public FormsLatexView() => InitializeComponent();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      var painter = new SkiaSharp.SkiaLatexPainter((float)Width, (float)Height) {
        BackgroundColor = BackgroundColor.ToSKColor(),
        DisplayErrorInline = DisplayErrorInline,
        ErrorColor = ErrorColor.ToSKColor(),
        ErrorFontSize = ErrorFontSize,
        FontSize = FontSize,
        LaTeX = LaTeX,
        Margin = new SkiaSharp.Thickness((float)Margin.Left, (float)Margin.Top, (float)Margin.Right, (float)Margin.Bottom),
        TextAlignment = (Enumerations.ColumnAlignment)TextAlignment,
        TextColor = TextColor.ToSKColor()
      };
      painter.Draw(e.Surface.Canvas);
      base.OnPaintSurface(e); 
    }
    
    public string ErrorMessage { get; private set; }
    public bool DisplayErrorInline { get; set; } = true;
    public float FontSize { get; set; } = 20f;
    public float? ErrorFontSize { get; set; } = null;
    public Color TextColor { get; set; } = Color.Black;
    public Color ErrorColor { get; set; } = Color.Red;
    public TextAlignment TextAlignment { get; set; } = TextAlignment.Start;
    public string LaTeX { get; set; }
  }
}