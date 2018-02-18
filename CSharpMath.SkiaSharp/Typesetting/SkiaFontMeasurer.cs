using Typography.OpenFont;
using CSharpMath.FrontEnd;
using TGlyph = System.Int32;

namespace CSharpMath.SkiaSharp
{
  public class SkiaFontMeasurer: IFontMeasurer<SkiaMathFont, TGlyph>
  {
    public int GetUnitsPerEm(SkiaMathFont font) => font.Typeface.UnitsPerEm;
  }
}
