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
      painter = painter ?? new SkiaSharp.SkiaLatexPainter((float)Width, (float)Height);
      painter.Draw(e.Surface.Canvas);
      painter.TextAlignment = Enumerations.ColumnAlignment.Center;
      painter.LaTeX = @"1 + 1";
      base.OnPaintSurface(e); 
    }
  }
}