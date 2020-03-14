using CSharpMath.Atoms;
using System.Drawing;
using Color = CSharpMath.Structures.Color;

namespace CSharpMath.Displays {
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
    /// <summary> iosMath name is "setTextColor".</summary> 
    void SetTextColorRecursive(Color? textColor);
    bool HasScript { get; set; }
  }
}
namespace CSharpMath {
  using Displays;
  using Displays.FrontEnd;
  partial class Extensions {
    /// <summary>The display's bounds, in its own coordinate system.</summary> 
    public static RectangleF DisplayBounds<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display) where TFont : IFont<TGlyph> =>
      new RectangleF(0, -display.Ascent, display.Width, display.Ascent + display.Descent);
    /// <summary>Where the display is located, expressed in its parent's coordinate system.</summary>
    public static RectangleF Frame<TFont, TGlyph>
      (this IDisplay<TFont, TGlyph> display) where TFont : IFont<TGlyph> =>
      display.DisplayBounds().Plus(display.Position);
  }
}
