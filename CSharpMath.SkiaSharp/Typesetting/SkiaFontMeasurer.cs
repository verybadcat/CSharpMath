using Typography.OpenFont;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;

namespace CSharpMath.SkiaSharp
{
  public class SkiaFontMeasurer: IFontMeasurer<SkiaMathFont, TGlyph>
  {
    public int GetUnitsPerEm(SkiaMathFont font) => font.Typeface.UnitsPerEm;
  }
}
