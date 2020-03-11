using CSharpMath.Atoms;
using CSharpMath.FrontEnd;

namespace CSharpMath.Tests.FrontEnd {
  class DoNothingFontChanger : IFontChanger {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();
    public int StyleCharacter(char c, FontStyle fontStyle) => c;
  }
}