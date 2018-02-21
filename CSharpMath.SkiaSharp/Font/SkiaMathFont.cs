using SkiaSharp;
using Typography.OpenFont;
using Typography.TextLayout;
using CSharpMath.Display;
using TGlyph = System.UInt16;
using System.Collections.Generic;

namespace CSharpMath.SkiaSharp {
  public class SkiaMathFont: MathFont<TGlyph>
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
      GlyphLayout = new GlyphLayout { Typeface = Typeface, FontSizeInPoints = PointSize };
    }

    public SkiaMathFont(SkiaMathFont cloneMe, float pointSize) :
      this(cloneMe.Name, cloneMe.Typeface, cloneMe.SKTypeface, pointSize) { }
  }
}
