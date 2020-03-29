namespace CSharpMath.CoreTests.FrontEnd {
  class DoNothingFontChanger : Display.FrontEnd.IFontChanger {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();
    public int StyleCharacter(char c, CSharpMath.Atom.FontStyle fontStyle) => c;
  }
}