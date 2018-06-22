using System;
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

    private TextAtom() { }

    public abstract IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts);

    private static IDisplay<MathFonts, Glyph> TextToDisplay(string content, MathFonts fonts, Range range) =>
        new TextRunDisplay<MathFonts, Glyph>(AttributedGlyphRuns.Create(content, GlyphFinder.Instance.FindGlyphs(fonts, content), fonts, false), range, TypesettingContext.Instance); 

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, Range range) => (Content, Range) = 
        (content.Any(c => char.IsWhiteSpace(c)) ? throw new ArgumentException("Content contains spaces or newlines.") : content, range);

      public string Content { get; }

      public Range Range { get; set; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) => TextToDisplay(Content, fonts, Range);
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, Range range) => (Content, Range) = (content, range);

      public Interfaces.IMathList Content { get; }

      public Range Range { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) =>
        Typesetter<MathFonts, Glyph>.CreateLine(Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
    }
    public sealed class Space : TextAtom {
      public Space(string content, Range range) => (Content, Range) = 
        (content.Any(c => !Spaces.Contains(c)) ? throw new ArgumentException("Content contains non-spaces.") : content, range);

      public string Content { get; }

      public Range Range { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) => TextToDisplay(Content, fonts, Range);
    }
    public sealed class Newline : TextAtom {
      public Newline(string content, Range range) => (Content, Range) = 
        (content.Any(c => !Newlines.Contains(c)) ? throw new ArgumentException("Content contains non-newlines.") : content, range);

      public string Content { get; }

      public Range Range { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) => TextToDisplay(Content, fonts, Range);
    }
    public sealed class List : TextAtom {
      public List(TextAtom[] content) => Content = content;

      public TextAtom[] Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts) {
        var displays = new IDisplay<MathFonts, Glyph>[Content.Length];
        for (int i = 0; i < Content.Length; i++) {
          displays[i] = Content[i].ToDisplay(fonts);
        }
        return new MathListDisplay<MathFonts, Glyph>(displays);
      }
    }
  }
}
