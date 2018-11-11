using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;

namespace CSharpMath.Tests.FrontEnd
{
  internal class DoNothingFontChanger : IFontChanger
  {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();

    public string ChangeFont(string inputString, FontStyle outputFontStyle) => inputString;
    public string ChangeGlyphs(string inputGlyphs, FontStyle outputFontStyle) => inputGlyphs;
  }
}