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

    public abstract IDisplay<Fonts, Glyph> ToDisplay(Fonts fonts);

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, int index) : base(new Range(index, content.Length)) => Content = content;

      public string Content { get; }

      public override IDisplay<Fonts, Glyph> ToDisplay(Fonts fonts) => 
        new TextRunDisplay<Fonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance);
    }
    public sealed class Newline : TextAtom {
      public Newline(string content, int index) : base(new Range(index, content.Length)) => Content = content;

      public string Content { get; }

      public override IDisplay<Fonts, Glyph> ToDisplay(Fonts fonts) => 
        new TextRunDisplay<Fonts, Glyph>(AttributedGlyphRuns.Create(Content, GlyphFinder.Instance.FindGlyphs(fonts, Content), fonts, false), Range, TypesettingContext.Instance);
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, bool displayStyle, Range range) : base(range) => (Content, DisplayStyle) = (content, displayStyle);

      public Interfaces.IMathList Content { get; }

      public bool DisplayStyle { get; }

      public override IDisplay<Fonts, Glyph> ToDisplay(Fonts fonts) =>
        Typesetter<Fonts, Glyph>.CreateLine(Content, fonts, TypesettingContext.Instance, DisplayStyle ? Enumerations.LineStyle.Display : Enumerations.LineStyle.Text);
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

      public override IDisplay<Fonts, Glyph> ToDisplay(Fonts fonts) {
        var displays = new IDisplay<Fonts, Glyph>[Content.Count];
        float X = 0f;
        for (int i = 0; i < Content.Count; i++) {
          displays[i] = Content[i].ToDisplay(fonts);
          X += displays[i].Width;
          displays[i].Position = new PointF(displays[i].Position.X + X, displays[i].Position.Y);
        }
        return new MathListDisplay<Fonts, Glyph>(displays);
      }
    }
  }
}
