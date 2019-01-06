using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CSharpMath.NuGetPackageTests.CSharp
{
	public partial class MainPage : ContentPage
	{
    public MainPage() {
      InitializeComponent();
      global::SkiaSharp.Views.Forms.SKCanvasView canvasView = canvas;
      CSharpMath.Forms.MathKeyboard keyboard = b;
      
      var painter = new CSharpMath.SkiaSharp.MathPainter
      { /*set all properties aside from LocalTypefaces, FontSize, LineStyle,
          MathList, LaTeX and Source (these are ignored)*/ };
      keyboard.RedrawRequested += (sender, e) => canvasView.InvalidateSurface();
      canvasView.PaintSurface += (sender, e) => {
        e.Surface.Canvas.Clear();
        //for any DrawDisplay overload, arguments after canvas are the same as Draw
        CSharpMath.SkiaSharp.MathPainter.DrawDisplay(painter, keyboard.Display, e.Surface.Canvas);
        keyboard.DrawCaret(e.Surface.Canvas, CSharpMath.Rendering.CaretShape.IBeam);
      };
		}
	}
}
