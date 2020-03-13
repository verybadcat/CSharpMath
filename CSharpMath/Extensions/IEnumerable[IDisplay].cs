using System.Collections.Generic;
using System.Linq;

namespace CSharpMath {
  using Displays;
  using Displays.FrontEnd;
  public static partial class Extensions {
    public static float CollectionAscent<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
        displays.IsNonEmpty() ? displays.Max(display => display.Ascent + display.Position.Y) : 0;
    public static float CollectionDescent<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
        displays.IsNonEmpty() ? displays.Max(display => display.Descent - display.Position.Y) : 0;
    public static float CollectionX<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
      displays.IsNonEmpty() ? displays.Min(d => d.Position.X) : 0;
    public static float CollectionMaxX<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
      displays.IsNonEmpty() ? displays.Max(d => d.Position.X + d.Width) : 0;
    public static float CollectionWidth<TFont, TGlyph>
      (this IEnumerable<IDisplay<TFont, TGlyph>> displays) where TFont : IFont<TGlyph> =>
      displays.CollectionMaxX() - displays.CollectionX();
  }
}
