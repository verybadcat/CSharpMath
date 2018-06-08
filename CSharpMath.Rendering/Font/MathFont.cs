using Typography.OpenFont;
using Typography.TextLayout;
using CSharpMath.Display;

namespace CSharpMath.Rendering {
  public class MathFont: MathFont<Glyph>
  {
    public string Name { get; }
    public GlyphLayout GlyphLayout { get; }
    public Typeface Typeface => GlyphLayout.Typeface;

    internal MathFont(string name, Typeface typeface, float pointSize) : base(pointSize)
    {
      Name = name;
      GlyphLayout = new GlyphLayout { Typeface = typeface };
    }

    public MathFont(MathFont cloneMe, float pointSize) : base(pointSize) {
      Name = cloneMe.Name;
      GlyphLayout = cloneMe.GlyphLayout;
    }
  }
}
