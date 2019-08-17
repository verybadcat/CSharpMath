using SkiaSharp;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;
namespace CSharpMath.Forms {
  public class SvgView : SKCanvasView {
    public SvgView() {
      EnableTouchEvents = true;
    }

    public System.IO.Stream Source { get; set; }
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e) {
      base.OnPaintSurface(e);
      if (Source is null) return;
      var svg = new global::SkiaSharp.Extended.Svg.SKSvg();
      svg.Load(Source);
      e.Surface.Canvas.DrawPicture(svg.Picture);
    }
    protected override void OnTouch(SKTouchEventArgs e) {
      base.OnTouch(e);
      if (e.ActionType == SKTouchAction.Pressed)
        Pressed(this, System.EventArgs.Empty);
      e.Handled = true;
    }
    public event System.EventHandler Pressed = delegate { };
  }
}
