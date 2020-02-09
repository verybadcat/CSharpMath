using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;

namespace CSharpMath.Tests.FrontEnd {
  class DoNothingFontChanger : IFontChanger {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();
    public string ChangeFont(string inputString, FontStyle _) => inputString;
    public string ChangeGlyphs(string inputGlyphs, FontStyle _) => inputGlyphs;
  }
}