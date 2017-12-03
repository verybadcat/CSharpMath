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
    public static TextLineDisplay<TGlyph> Create<TGlyph>(
      AttributedString<TGlyph> text,
      Range range,
      TypesettingContext<TGlyph> context,
      IEnumerable<IMathAtom> atoms
      ) {
      int index = range.Location;
      List<TextRunDisplay<TGlyph>> textRuns = new List<TextRunDisplay<TGlyph>>();
      foreach (var run in text.Runs) {
        var innerRange = new Range(index, run.Length);
        var textRun = new TextRunDisplay<TGlyph>(
          run,
          innerRange,
          context
          );
        textRuns.Add(textRun);
      }
      return new TextLineDisplay<TGlyph>(textRuns, atoms);
    }
  }
}
