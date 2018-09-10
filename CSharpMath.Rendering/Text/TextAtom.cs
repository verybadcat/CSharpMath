using System.Collections.Generic;

namespace CSharpMath.Rendering {
  using Range = Atoms.Range;
  using Enumerations;

#warning Review Ranges; they have an extremely high probability of not working
  //Base type
  public abstract class TextAtom {
    private TextAtom(Range range) => Range = range;

    public Range Range { get; private set; }

    public abstract int? SingleChar(FontStyle style);

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, int index) : base(new Range(index, content.Length)) =>
        Content = string.IsNullOrWhiteSpace(content) ? throw new System.ArgumentException("Null, an empty string or whitespace was provided.") : content;

      public string Content { get; private set; }

      public override int? SingleChar(FontStyle style) =>
        Typesetter<Fonts, Glyph>.UnicodeLengthIsOne(Content) ? UnicodeFontChanger.Instance.ChangeFont(GlyphFinder.Instance.GetCodepoint(Content, 0), style) : new int?();

      internal void Append(string moreText) => Content += moreText;
    }
    public sealed class Newline : TextAtom {
      public Newline(int index, int length) : base(new Range(index, length)) { }
      public override int? SingleChar(FontStyle style) => null;
    }
    public sealed class Space : TextAtom {
      public Space(Structures.Space space, int index, int length) : base(new Range(index, length)) => Content = space;
      public Structures.Space Content { get; }
      public override int? SingleChar(FontStyle style) => ' ';
  }
    public sealed class ControlSpace : TextAtom {
      public ControlSpace(int index) : base(new Range(index, 2)) { } // backslash + whitespace = 2 characters
      public override int? SingleChar(FontStyle style) => ' ';
    }
    public sealed class Accent : TextAtom {
      public Accent(TextAtom content, string accent, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, AccentChar, content.Range) =
          (content, accent, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public string AccentChar { get; }

      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, bool displayStyle, Range range) : base(range) => (Content, DisplayStyle) = (content, displayStyle);

      public Interfaces.IMathList Content { get; }

      public bool DisplayStyle { get; }

      public override int? SingleChar(FontStyle style) => null;
    }
    public sealed class Style : TextAtom {
      public Style(TextAtom content, FontStyle style, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, FontStyle, content.Range) =
          (content, style == FontStyle.Default ? FontStyle.Roman /*FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters*/ : style, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public FontStyle FontStyle { get; }

      public override int? SingleChar(FontStyle style) => Content.SingleChar(FontStyle);
    }
    public sealed class Size : TextAtom {
      public Size(TextAtom content, float pointSize, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, PointSize, content.Range) = (content, pointSize, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public float PointSize { get; }

      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
    }
    public sealed class Color : TextAtom {
      public Color(TextAtom content, Structures.Color colour, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, Colour, content.Range) = (content, colour, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public Structures.Color Colour { get; }
      
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
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

      public override int? SingleChar(FontStyle style) => Content.Count == 1 ? Content[0].SingleChar(style) : null;
    }
  }
}
