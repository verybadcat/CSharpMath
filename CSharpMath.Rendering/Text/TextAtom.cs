using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Linq.Enumerable;

namespace CSharpMath.Rendering {
  using Range = Atoms.Range;
  using Display;
  using Display.Text;

  [Obsolete("The Text classes are not yet usable in this prerelease.", true)]
  //Base type
  public abstract class TextAtom {
    private TextAtom(Range range) => Range = range;

    public Range Range { get; private set; }

    public abstract IPositionableDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts);

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, int index) : base(new Range(index, content.Length)) => Content = content;

      public string Content { get; }

      public override IPositionableDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) => 
        new TextRunDisplay<MathFonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance);
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, Range range) : base(range) => Content = content;

      public Interfaces.IMathList Content { get; }

      public override IPositionableDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) =>
        Typesetter<MathFonts, Glyph>.CreateLine(Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
    }
    public sealed class List : TextAtom {
      public List(IReadOnlyList<TextAtom> content, int index) : base(new Range(index, content.Count)) {
        Content = content;
        Offset(index);
      }

      private void Offset(int offset) {
        foreach(var atom in Content) {
          if (atom is List l) l.Offset(offset);
          else atom.Range = new Range(atom.Range.Location + offset, atom.Range.Length);
        }
      }

      public IReadOnlyList<TextAtom> Content { get; }

      public override IPositionableDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) {
        var displays = new IDisplay<MathFonts, Glyph>[Content.Count];
        for (int i = 0; i < Content.Count; i++) {
          displays[i] = Content[i].ToDisplay(fonts);
        }
        return new MathListDisplay<MathFonts, Glyph>(displays);
      }
    }
  }
}
