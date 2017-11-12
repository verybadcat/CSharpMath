using CSharpMath.Atoms;
using System.Drawing;

namespace CSharpMath {
  public interface IDisplay {
    void Draw(IGraphicsContext context);
    RectangleF DisplayBounds { get; }
    float Ascent { get; }
    float Descent { get; }
    float Width { get; }
    PointF Position { get; set; }
    Range Range { get; }
    bool HasScript { get; }
    Color? TextColor { get; }
    Color? LocalTextColor { get; }
  }
}
