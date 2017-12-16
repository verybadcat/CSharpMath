using CSharpMath.FrontEnd;

namespace CSharpMath.Tests.FrontEnd
{
  internal class DoNothingFontChanger : IFontChanger
  {
    public DoNothingFontChanger()
    {
    }

    public string ChangeGlyphs (string inputGlyphs, FontStyle outputFontStyle)
    {
      return inputGlyphs;
    }
  }
}