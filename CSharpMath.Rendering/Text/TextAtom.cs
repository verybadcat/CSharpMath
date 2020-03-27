using System.Collections.Generic;

namespace CSharpMath.Rendering.Text {
  using Atom;
  //Base type
  public abstract class TextAtom {
    public abstract int? SingleChar(FontStyle style);
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
        ? Display.UnicodeFontChanger.Instance.StyleCharacter(Content[0], style)
        : new int?();
    }
    public sealed class Newline : TextAtom {
      public override int? SingleChar(FontStyle style) => null;
    }
    public sealed class Space : TextAtom {
      public Space(Structures.Space space) => Content = space;
      public Structures.Space Content { get; }
      public override int? SingleChar(FontStyle style) => ' ';
    }
    public sealed class ControlSpace : TextAtom {
      public override int? SingleChar(FontStyle style) => ' ';
    }
    public sealed class Accent : TextAtom {
      public Accent(TextAtom content, string accent) =>
        (Content, AccentChar) = (content, accent);
      public TextAtom Content { get; }
      public string AccentChar { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
    }
    public sealed class Math : TextAtom {
      public Math(MathList content, bool displayStyle) =>
        (Content, DisplayStyle) = (content, displayStyle);
      public MathList Content { get; }
      public bool DisplayStyle { get; }
      public override int? SingleChar(FontStyle style) => null;
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
    }
    public sealed class Size : TextAtom {
      public Size(TextAtom content, float pointSize) => (Content, PointSize) = (content, pointSize);
      public TextAtom Content { get; }
      public float PointSize { get; }
      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
    }
    public sealed class Color : TextAtom {
      public Color(TextAtom content, Structures.Color colour) => (Content, Colour) = (content, colour);
      public TextAtom Content { get; }
      public Structures.Color Colour { get; }

      public override int? SingleChar(FontStyle style) => Content.SingleChar(style);
    }
    public sealed class List : TextAtom {
      public List(IReadOnlyList<TextAtom> content) => Content = content;
      public IReadOnlyList<TextAtom> Content { get; }
      public override int? SingleChar(FontStyle style) =>
        Content.Count == 1 ? Content[0].SingleChar(style) : null;
    }
    public sealed class Comment : TextAtom {
      public Comment(string comment) => Content = comment;
      public string Content { get; }
      public override int? SingleChar(FontStyle style) => null;
    }
  }
}
