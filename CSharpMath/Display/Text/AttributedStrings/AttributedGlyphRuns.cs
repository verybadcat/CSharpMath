using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Display.Text {
  public static class AttributedGlyphRuns {
    public static AttributedGlyphRun Create(string text, MathFont font, Color color)
      => new AttributedGlyphRun {
        Text = text,
        Font = font,
        TextColor = color
      };

    public static AttributedGlyphRun Create(string text, MathFont font)
      => new AttributedGlyphRun {
        Text = text,
        Font = font
      };

    public static AttributedGlyphRun Create(string text, Color color)
      => new AttributedGlyphRun {
        Text = text,
        TextColor = color
      };
  }
}
