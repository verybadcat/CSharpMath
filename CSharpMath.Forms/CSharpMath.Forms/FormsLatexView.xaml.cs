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
    SkiaSharp.SkiaLatexPainter painter;

    public FormsLatexView() => InitializeComponent();

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      painter = painter ?? new SkiaSharp.SkiaLatexPainter(e.Info.Size);
      painter.BoundsSK = e.Info.Size;
      painter.LaTeX = @"1 + 1";
      painter.Draw(e.Surface.Canvas);
      base.OnPaintSurface(e); 
    }
  }
}