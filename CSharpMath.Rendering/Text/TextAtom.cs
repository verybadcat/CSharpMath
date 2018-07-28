using System.Collections.Generic;

namespace CSharpMath.Rendering {
  using Range = Atoms.Range;
  using Display;
  using Display.Text;
  using System.Drawing;

  //Base type
  public abstract class TextAtom {
    private TextAtom(Range range) => Range = range;

    public Range Range { get; private set; }

    public abstract IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts, PointF position);

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, int index) : base(new Range(index, content.Length)) => Content = content;

      public string Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts, PointF position) => 
        new TextRunDisplay<MathFonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance) { Position = position };
    }
    public sealed class Newline : TextAtom {
      public Newline(string content, int index) : base(new Range(index, content.Length)) => Content = content;

      public string Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts, PointF position) => 
        new TextRunDisplay<MathFonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance) { Position = position };
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, Range range) : base(range) => Content = content;

      public Interfaces.IMathList Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts, PointF position) {
        var p = Typesetter<MathFonts, Glyph>.CreateLine(Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
        p.Position = position;
        return p;
      }
    }
    public sealed class List : TextAtom {
      public List(IReadOnlyList<TextAtom> content, int index) : base(new Range(index, content.Count)) {
        Content = content;
        if(index != 0) Offset(index);
      }

      private void Offset(int offset) {
        foreach(var atom in Content) {
          if (atom is List l) l.Offset(offset);
          else atom.Range = new Range(atom.Range.Location + offset, atom.Range.Length);
        }
      }

      public IReadOnlyList<TextAtom> Content { get; }

      public override IDisplay<MathFonts, Glyph> ToDisplay(MathFonts fonts, PointF position) {
        var displays = new IDisplay<MathFonts, Glyph>[Content.Count];
        for (int i = 0; i < Content.Count; i++) {
          displays[i] = Content[i].ToDisplay(fonts, position);
          position.X += displays[i].Width;
        }
        return new MathListDisplay<MathFonts, Glyph>(displays);
      }
    }
  }
}
