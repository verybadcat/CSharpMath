using SkiaSharp;
using Typography.OpenFont;
using Typography.TextLayout;
using CSharpMath.Display;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathFont: MathFont<Glyph>
  {
    public string Name { get; }
    public Typeface Typeface { get; }
    public SKTypeface SKTypeface { get; }
    public GlyphLayout GlyphLayout { get; }

    internal SkiaMathFont(string name, Typeface typeface, SKTypeface skTypeface, float pointSize) : base(pointSize)
    {
      Name = name;
      Typeface = typeface;
      SKTypeface = skTypeface;
      GlyphLayout = new GlyphLayout { Typeface = Typeface };
    }

    public SkiaMathFont(SkiaMathFont cloneMe, float pointSize) :
      this(cloneMe.Name, cloneMe.Typeface, cloneMe.SKTypeface, pointSize) { }
  }
}
