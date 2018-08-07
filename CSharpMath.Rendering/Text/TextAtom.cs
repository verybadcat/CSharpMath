using System.Collections.Generic;

namespace CSharpMath.Rendering {
  using Range = Atoms.Range;
  using Display;
  using Display.Text;
  using System.Drawing;
  using CSharpMath.Atoms;

#warning Review Ranges; they have an extremely high probability of not working
  //Base type
  public abstract class TextAtom {
    private TextAtom(Range range) => Range = range;

    public Range Range { get; private set; }

    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content, int index) : base(new Range(index, content.Length)) =>
        Content = string.IsNullOrEmpty(content) ? throw new System.ArgumentException("Null or an empty string was provided.") : content;

      public string Content { get; }
    }
    public sealed class Newline : TextAtom {
      public Newline(int index, int length) : base(new Range(index, length)) { }
    }
    public sealed class Math : TextAtom {
      public Math(Interfaces.IMathList content, bool displayStyle, Range range) : base(range) => (Content, DisplayStyle) = (content, displayStyle);

      public Interfaces.IMathList Content { get; }

      public bool DisplayStyle { get; }
    }
    public sealed class Style : TextAtom {
      public Style(TextAtom content, FontStyle style, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, FontStyle, content.Range) =
          (content, style == FontStyle.Default ? FontStyle.Roman /*FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters*/ : style, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public FontStyle FontStyle { get; }
    }
    public sealed class Size : TextAtom {
      public Size(TextAtom content, float pointSize, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, PointSize, content.Range) = (content, pointSize, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public float PointSize { get; }
    }
    public sealed class Color : TextAtom {
      public Color(TextAtom content, Structures.Color colour, int index, int commandLength) : base(new Range(index, commandLength + content.Range.Length + 2 /*{ and }*/)) =>
        (Content, Colour, content.Range) = (content, colour, new Range(content.Range.Location + commandLength + index + 1/*{*/, content.Range.Length));

      public TextAtom Content { get; }

      public Structures.Color Colour { get; }
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
    }
  }
}
