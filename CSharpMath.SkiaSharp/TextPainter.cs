using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;
using System.Drawing;

namespace CSharpMath.SkiaSharp {
  public class TextPainter : TextPainter<SKCanvas, SKColor> {
    ///<summary>Set this to false for not rendering partially blended pixels that make edges look smooth.</summary>
    public bool AntiAlias { get; set; } = true;
    public override Color WrapColor(SKColor color) => color.FromNative();
    public override SKColor UnwrapColor(Color color) => color.ToNative();
    public override ICanvas WrapCanvas(SKCanvas canvas) => new SkiaCanvas(canvas, AntiAlias);
  }
}
