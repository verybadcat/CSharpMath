namespace CSharpMath.Tests.FrontEnd {
  class DoNothingFontChanger : Displays.FrontEnd.IFontChanger {
    DoNothingFontChanger() { }
    public static DoNothingFontChanger Instance { get; } = new DoNothingFontChanger();
    public int StyleCharacter(char c, Atoms.FontStyle fontStyle) => c;
  }
}