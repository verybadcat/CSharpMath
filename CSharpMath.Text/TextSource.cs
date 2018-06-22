namespace CSharpMath.Text {
  public readonly struct TextSource {
    public TextSource(string text) {
      Text = text;
      Atom = TextBuilder.Build(text);
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      Text = TextBuilder.Unbuild(atom, new System.Text.StringBuilder()).ToString();
    }

    public string Text { get; }
    public TextAtom Atom { get; }
  }
}
