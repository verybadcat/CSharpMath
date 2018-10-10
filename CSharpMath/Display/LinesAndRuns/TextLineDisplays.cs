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
    public static TextLineDisplay<TFont, TGlyph> Create<TFont, TGlyph>(
      AttributedString<TFont, TGlyph> text,
      Range range,
      TypesettingContext<TFont, TGlyph> context,
      List<IMathAtom> atoms
      )
      where TFont: IFont<TGlyph> {
      int index = range.Location;
      List<TextRunDisplay<TFont, TGlyph>> textRuns = new List<TextRunDisplay<TFont, TGlyph>>();
      foreach (var run in text.Runs) {
        var innerRange = new Range(index, run.Length);
        var textRun = new TextRunDisplay<TFont, TGlyph>(
          run,
          innerRange,
          context
          );
        textRuns.Add(textRun);
      }
      return new TextLineDisplay<TFont, TGlyph>(textRuns, atoms);
    }
  }
}
