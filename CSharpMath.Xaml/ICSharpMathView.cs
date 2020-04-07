using CSharpMath.Rendering.FrontEnd;
using CSharpMath.Structures;

#if Avalonia
namespace CSharpMath.Avalonia {
#elif Forms
namespace CSharpMath.Forms {
#endif
  public interface ICSharpMathView<TContent, TColor, TCanvasColor, TCanvas, TPainter>
    //: ICSharpMathAPI<TContent, TColor>
    where TPainter : Painter<TCanvas, TContent, TCanvasColor>
    where TContent : class {
    TPainter Painter { get; }
    //TextAlignment TextAlignment { get; set; }
    //Thickness Padding { get; set; }
    //float DisplacementX { get; set; }
    //float DisplacementY { get; set; }
  }
}
