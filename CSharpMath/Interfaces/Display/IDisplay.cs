using CSharpMath.Atoms;
using System.Drawing;
using CSharpMath.Display;
using CSharpMath.FrontEnd;

namespace CSharpMath {
  public interface IDisplay<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    void Draw(IGraphicsContext<TFont, TGlyph> context);
    /// <summary>The display's bounds, in its own coordinate system.</summary> 
    RectangleF DisplayBounds { get; }

    /// <summary>By convention, Ascent and Descent should be positive
    /// numbers for the typical case where your font is partly above
    /// and partly below the baseline. This may differ from the 
    /// convention of a particular OS, i.e. iOS.</summary>
    float Ascent { get; }
    float Descent { get; }
    float Width { get; }
    Range Range { get; }
    /// <summary>Position of the display, relative to its parent.</summary> 
    PointF Position { get; }

    bool HasScript { get; set; }
  }

  public static class IDisplayExtensions {
    public static RectangleF ComputeDisplayBounds<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display)
      where TFont : MathFont<TGlyph>
      => new RectangleF(0, -display.Ascent, display.Width, display.Ascent + display.Descent);
    /// <summary>Where the display is located, expressed in its parent's coordinate system.</summary>
    public static RectangleF Frame<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display)
      where TFont : MathFont<TGlyph>
      => display.DisplayBounds.Plus(display.Position);
  }
}
