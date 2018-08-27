using System;
using CSharpMath.Display.Text;
using CSharpMath.FrontEnd;
using TGlyph = System.UInt16;

namespace CSharpMath.Apple
{
  public class AppleFontMeasurer: FrontEnd.IFontMeasurer<AppleMathFont, TGlyph>
  {
    private AppleFontMeasurer()
    {
    }

    public static AppleFontMeasurer Instance { get; } = new AppleFontMeasurer();

    public int GetUnitsPerEm(AppleMathFont font)
    {
      return (int)font.CtFont.UnitsPerEmMetric;
    }
  }
}
