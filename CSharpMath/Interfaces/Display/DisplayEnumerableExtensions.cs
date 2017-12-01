using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath {
  public static class DisplayEnumerableExtensions {
    public static float CollectionAscent(this IEnumerable<IDisplay> displays) {
      float r = 0;
      foreach (var display in displays) {
        r = Math.Max(r, display.Ascent + display.Position.Y);
      }
      return r;
    }
    public static float CollectionDescent(this IEnumerable<IDisplay> displays) {
      float r = 0;
      foreach (var display in displays) {
        r = Math.Max(r, display.Descent - display.Position.Y);
      }
      return r;
    }

    public static float CollectionX(this IEnumerable<IDisplay> displays)
      => displays.Min(d => d.Position.X);
    public static float CollectionMaxX(this IEnumerable<IDisplay> displays)
      => displays.Max(d => d.Position.X + d.Width);

    public static float CollectionWidth(this IEnumerable<IDisplay> displays)
      => displays.CollectionMaxX() - displays.CollectionX();
  }
}
