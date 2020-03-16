namespace CSharpMath.Tests.FrontEnd {
  class DoNothingFontChanger : Display.FrontEnd.IFontChanger {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();
    public int StyleCharacter(char c, Atom.FontStyle fontStyle) => c;
  }
}