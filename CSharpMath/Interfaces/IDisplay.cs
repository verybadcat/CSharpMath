using CSharpMath.Atoms;
using System.Drawing;

namespace CSharpMath {
  public interface IDisplay {
    void Draw(IGraphicsContext context);
    RectangleF DisplayBounds { get; }
    float Ascent { get; }
    float Descent { get; }
    float Width { get; }
    Range Range { get; }
  }

  public static class IDisplayExtensions {
    public static RectangleF OriginBoundsFromAscentDescentWidth(this IDisplay display)
      => new RectangleF(0, -display.Ascent, display.Width, display.Ascent + display.Descent);
  }
}
