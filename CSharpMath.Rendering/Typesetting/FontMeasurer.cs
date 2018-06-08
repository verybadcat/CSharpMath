using Typography.OpenFont;
using CSharpMath.FrontEnd;

namespace CSharpMath.Rendering {
  public class FontMeasurer: IFontMeasurer<MathFont, Glyph>
  {
    public int GetUnitsPerEm(MathFont font) => font.Typeface.UnitsPerEm;
  }
}
