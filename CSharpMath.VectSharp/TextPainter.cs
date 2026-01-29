using System.Drawing;
using CSharpMath.Rendering.FrontEnd;

namespace CSharpMath.VectSharp;

public class TextPainter : TextPainter<global::VectSharp.Page, global::VectSharp.Colour> {
  public override Color WrapColor(global::VectSharp.Colour color) => color.FromNative();
  public override global::VectSharp.Colour UnwrapColor(Color color) => color.ToNative();
  public override ICanvas WrapCanvas(global::VectSharp.Page canvas) => new VectSharpCanvas(canvas);
}