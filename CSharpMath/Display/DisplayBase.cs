using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>
  /// Corresponds to MTDisplay in iosMath.
  /// </summary>
  public class DisplayBase : IDisplay {
    public float Ascent { get; protected set; }
    public float Descent { get; protected set; }
    public float Width { get; set; }
    public Range Range { get; protected set; }
    public bool HasScript { get; protected set; }
    public PointF Position { get; set; }
    public Color? TextColor { get;  set; }
    public Color? LocalTextColor { get; set; }
    public RectangleF DisplayBounds => new RectangleF(
      this.Position.X,
      this.Position.Y,
      this.Width,
      this.Ascent + this.Descent
      );

    public virtual void Draw(IGraphicsContext context) {
    }
  }
}
