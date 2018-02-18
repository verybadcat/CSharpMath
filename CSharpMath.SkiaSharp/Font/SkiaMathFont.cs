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
    public float FontSizeInPoints { get; }
    public GlyphLayout GlyphLayout => new GlyphLayout { Typeface = Typeface, FontSizeInPoints = FontSizeInPoints };
    public SKTypeface SKTypeface { get; }

    internal SkiaMathFont(string name, Typeface typeface, SKTypeface skTypeface, float pointSize) : base(pointSize)
    {
      Name = name;
      Typeface = typeface;
      SKTypeface = skTypeface;
      FontSizeInPoints = pointSize;
    }

    public SkiaMathFont(SkiaMathFont cloneMe, float pointSize): base(pointSize) {
      Name = cloneMe.Name;
      Typeface = cloneMe.Typeface;
      SKTypeface = cloneMe.SKTypeface;
      FontSizeInPoints = pointSize;
    }
  }
}
