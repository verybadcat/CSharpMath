using System;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple
{
  public class AppleFontMeasurer: IFontMeasurer<AppleMathFont, TGlyph>
  {
    public AppleFontMeasurer()
    {
    }

    public int GetUnitsPerEm(AppleMathFont font)
    {
      return (int)font.CtFont.UnitsPerEmMetric;
    }
  }
}
