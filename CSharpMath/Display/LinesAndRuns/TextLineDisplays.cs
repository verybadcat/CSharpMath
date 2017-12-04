using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  public static class TextLineDisplays {
    public static TextLineDisplay<TMathFont, TGlyph> Create<TMathFont, TGlyph>(
      AttributedString<TMathFont, TGlyph> text,
      Range range,
      TypesettingContext<TMathFont, TGlyph> context,
      IEnumerable<IMathAtom> atoms
      )
      where TMathFont: MathFont<TGlyph> {
      int index = range.Location;
      List<TextRunDisplay<TMathFont, TGlyph>> textRuns = new List<TextRunDisplay<TMathFont, TGlyph>>();
      foreach (var run in text.Runs) {
        var innerRange = new Range(index, run.Length);
        var textRun = new TextRunDisplay<TMathFont, TGlyph>(
          run,
          innerRange,
          context
          );
        textRuns.Add(textRun);
      }
      return new TextLineDisplay<TMathFont, TGlyph>(textRuns, atoms);
    }
  }
}
