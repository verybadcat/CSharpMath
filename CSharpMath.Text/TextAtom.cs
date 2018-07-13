using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static System.Linq.Enumerable;

namespace CSharpMath.Text {
  using Range = Atoms.Range;
  using Display;
  using Display.Text;
  using Rendering;

  //Base type
  public abstract class TextAtom {
    public static readonly ReadOnlyCollection<char> Spaces = new ReadOnlyCollection<char>(new[] {
      '\u0020', '\u00a0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004','\u2005','\u2006','\u2007','\u2008','\u2009','\u200a','\u202f','\u205f','\u3000'
    });

    public static readonly ReadOnlyCollection<char> Newlines = new ReadOnlyCollection<char>(new[] {
      '\u000a', '\u000b', '\u000c', '\u000d', '\u0085', '\u2028', '\u2029'
    });

    private TextAtom(Range range) => Range = range;

    public Range Range { get; }

    public abstract IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts);

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, Range range) : base(range) => Content = content;

      public string Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) => 
        new TextRunDisplay<MathFonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance);
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, Range range) : base(range) => Content = content;

      public Interfaces.IMathList Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) =>
        Typesetter<MathFonts, Glyph>.CreateLine(Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
    }
    public sealed class List : TextAtom {
      public List(IReadOnlyList<TextAtom> content, int startAt) : base(new Range(startAt, content.Count)) {
        Content = content;
        Offset(startAt);
      }

      private void Offset(int offset) {
        foreach(var atom in Content) {
          if (atom is List l) l.Offset(offset);
          else atom.Range = new Range(atom.Range.Location + offset, atom.Range.Length);
        }
      }

      public IReadOnlyList<TextAtom> Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) {
        var displays = new IDisplay<MathFonts, Glyph>[Content.Count];
        for (int i = 0; i < Content.Count; i++) {
          displays[i] = Content[i].ToDisplay(fonts);
        }
        return new MathListDisplay<MathFonts, Glyph>(displays);
      }
    }
  }
}
