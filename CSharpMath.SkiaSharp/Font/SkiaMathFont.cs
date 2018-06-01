using SkiaSharp;
using Typography.OpenFont;
using Typography.TextLayout;
using CSharpMath.Display;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathFont: MathFont<Glyph>
  {
    public string Name { get; }
    public GlyphLayout GlyphLayout { get; }
    public Typeface Typeface => GlyphLayout.Typeface;

    internal SkiaMathFont(string name, Typeface typeface, float pointSize) : base(pointSize)
    {
      Name = name;
      GlyphLayout = new GlyphLayout { Typeface = typeface };
    }

    public SkiaMathFont(SkiaMathFont cloneMe, float pointSize) : base(pointSize) {
      Name = cloneMe.Name;
      GlyphLayout = cloneMe.GlyphLayout;
    }
  }
}
