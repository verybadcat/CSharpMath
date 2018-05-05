using Typography.OpenFont;
using CSharpMath.FrontEnd;

namespace CSharpMath.SkiaSharp
{
  public class SkiaFontMeasurer: IFontMeasurer<SkiaMathFont, Glyph>
  {
    public int GetUnitsPerEm(SkiaMathFont font) => font.Typeface.UnitsPerEm;
  }
}
