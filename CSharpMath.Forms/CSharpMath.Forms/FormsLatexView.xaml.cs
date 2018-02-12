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
    readonly SkiaSharp.SkiaLatexPainter painter = new SkiaSharp.SkiaLatexPainter();

    public FormsLatexView() => InitializeComponent();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      painter.PaintSurface(e.Surface, e.Info);
      base.OnPaintSurface(e); 
    }
  }
}