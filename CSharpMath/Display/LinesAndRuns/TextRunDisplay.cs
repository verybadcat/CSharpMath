using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath. Will need to
  /// figure out Core Text a bit in order to fill this out.</summary> 
  public class TextRunDisplay : DisplayBase {
    public TextLine Line { get; private set; }
    public AttributedGlyphRun Run { get; private set; }

    public TextRunDisplay(
      AttributedGlyphRun run, 
      Range range, 
      TypesettingContext context) {
      var font = run.Font;
      Run = run;
      Range = range;
      var bounds = context.GlyphBoundsProvider.GetBoundingRectForGlyphs(font, run.Text);
      Width = bounds.Width;
      Ascent = Math.Max(0, bounds.Top);
      Descent = Math.Max(0, -bounds.Bottom);
    }
  }
}
