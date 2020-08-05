using CSharpMath.Atom;
using System.Drawing;

namespace CSharpMath.Display {
  using FrontEnd;
  public interface IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    void Draw(IGraphicsContext<TFont, TGlyph> context);
    /// <summary>By convention, Ascent and Descent should be positive
    /// numbers for the typical case where your font is partly above
    /// and partly below the baseline. This may differ from the 
    /// convention of a particular OS, i.e. iOS.</summary>
    float Ascent { get; }
    float Descent { get; }
    float Width { get; }
    Range Range { get; }
    /// <summary>Position of the display, relative to its parent.</summary> 
    PointF Position { get; set; }
    Color? TextColor { get; set; }
    void SetTextColorRecursive(Color? textColor);
    Color? BackColor { get; set; }
    bool HasScript { get; set; }
  }
}
namespace CSharpMath {
  using Display;
  using Display.FrontEnd;
  partial class Extensions {
    /// <summary>
    /// The display's bounds, in its own coordinate system.<br/>
    /// **Internal only! The Rectangle's position is at the bottom left: not what the outer world expects**
    /// </summary> 
    public static RectangleF DisplayBounds<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display) where TFont : IFont<TGlyph> =>
      new RectangleF(0, -display.Descent, display.Width, display.Ascent + display.Descent);
    /// <summary>Where the display is located, expressed in its parent's coordinate system.</summary>
    public static RectangleF Frame<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display) where TFont : IFont<TGlyph> =>
      display.DisplayBounds().Plus(display.Position);
    public static void DrawBackground<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display, IGraphicsContext<TFont, TGlyph> context)
      where TFont : IFont<TGlyph> {
      if (display.BackColor is { } color) {
        context.SaveState();
        context.FillRect(display.Frame(), color);
        context.RestoreState();
      }
    }
  }
}
