using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Rendering.Text {
  using Atom;
  //Base type
  public abstract class TextAtom : System.IEquatable<TextAtom> {
    public abstract int? SingleChar(FontStyle style);
    public abstract bool Equals(TextAtom a);
    public override bool Equals(object obj) => obj is TextAtom a && Equals(a);
    public abstract override int GetHashCode();
    //Concrete types
    public sealed class Text : TextAtom {
      public Text(string content) =>
        Content =
          string.IsNullOrWhiteSpace(content)
          ? throw new System.ArgumentException("Null, an empty string or whitespace was provided.")
          : content;
      public string Content { get; }
      public override int? SingleChar(FontStyle style) =>
        Display.Typesetter.UnicodeLengthIsOne(Content)
        ? Display.UnicodeFontChanger.StyleCharacter(Content[0], style)
        : new int?();
      public override bool Equals(TextAtom atom) => atom is Text t && t.Content == Content;
      public override int GetHashCode() => Content.GetHashCode();
    }
    public sealed class Newline : TextAtom {
      public override int? SingleChar(FontStyle style) => null;
      public override bool Equals(TextAtom atom) => atom is Newline;
      public override int GetHashCode() => "\r\n".GetHashCode();
    }
    public sealed class Space : TextAtom {
      public Space(Structures.Space space) => Content = space;
      public Structures.Space Content { get; }
      public override int? SingleChar(FontStyle style) => ' ';
      public override bool Equals(TextAtom atom) => atom is Space s && s.Content == Content;
      public override int GetHashCode() => Content.GetHashCode();
    }
    public sealed class ControlSpace : TextAtom {
      public override int? SingleChar(FontStyle style) => ' ';
      public override bool Equals(TextAtom atom) => atom is ControlSpace;
      public override int GetHashCode() => " ".GetHashCode();
    }
    public sealed class Accent : TextAtom {
      public Accent(TextAtom content, string accent) =>
        (Content, AccentChar) = (content, accent);
      public TextAtom Content { get; }
      public string AccentChar { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
      public override bool Equals(TextAtom atom) => atom is Accent a && a.AccentChar == AccentChar && a.Content.Equals(Content);
      public override int GetHashCode() => (AccentChar, Content).GetHashCode();
    }
    public sealed class Math : TextAtom {
      public Math(MathList content, bool displayStyle) =>
        (Content, DisplayStyle) = (content, displayStyle);
      public MathList Content { get; }
      public bool DisplayStyle { get; }
      public override int? SingleChar(FontStyle style) => null;
      public override bool Equals(TextAtom atom) => atom is Math m && m.Content.Equals(Content) && m.DisplayStyle == DisplayStyle;
      public override int GetHashCode() => (DisplayStyle, Content).GetHashCode();
    }
    public sealed class Style : TextAtom {
      public Style(TextAtom content, FontStyle style) =>
        (Content, FontStyle) =
          (content, style == FontStyle.Default
           //FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters
           ? FontStyle.Roman
           : style);
      public TextAtom Content { get; }
      public FontStyle FontStyle { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(FontStyle);
      public override bool Equals(TextAtom atom) => atom is Style s && s.FontStyle == FontStyle && s.Content.Equals(Content);
      public override int GetHashCode() => (FontStyle, Content).GetHashCode();
    }
    public sealed class Size : TextAtom {
      public Size(TextAtom content, float pointSize) => (Content, PointSize) = (content, pointSize);
      public TextAtom Content { get; }
      public float PointSize { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
      public override bool Equals(TextAtom atom) => atom is Size s && s.PointSize == PointSize && s.Content.Equals(Content);
      public override int GetHashCode() => (PointSize, Content).GetHashCode();
    }
    public sealed class Colored : TextAtom {
      public Colored(TextAtom content, System.Drawing.Color colour) => (Content, Colour) = (content, colour);
      public TextAtom Content { get; }
      public System.Drawing.Color Colour { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
      public override bool Equals(TextAtom atom) => atom is Colored c && c.Colour == Colour && c.Content.Equals(Content);
      public override int GetHashCode() => (Colour, Content).GetHashCode();
    }
    public sealed class List : TextAtom {
      public List(IReadOnlyList<TextAtom> content) => Content = content;
      public IReadOnlyList<TextAtom> Content { get; }
      public override int? SingleChar(FontStyle style) =>
        Content.Count == 1 ? Content[0].SingleChar(style) : null;
      public override bool Equals(TextAtom atom) =>
        atom is List l
        && l.Content.Count == Content.Count
        && l.Content.Zip(Content, (a1, a2) => a1.Equals(a2)).All(b => b);
      public override int GetHashCode() => Content.Aggregate(0, (a, c) => a ^ c.GetHashCode()).GetHashCode();
    }
    public sealed class Comment : TextAtom {
      public Comment(string comment) => Content = comment;
      public string Content { get; }
      public override int? SingleChar(FontStyle style) => null;
      public override bool Equals(TextAtom atom) => atom is Comment c && c.Content == Content;
      public override int GetHashCode() => Content.GetHashCode();
    }
  }
}
