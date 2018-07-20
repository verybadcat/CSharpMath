namespace CSharpMath.Rendering {
  [System.Obsolete("The Text classes are not yet usable in this prerelease.", true)]
  public readonly struct TextSource {
    public TextSource(string text) {
      Text = text;
      (Atom, Error) = TextBuilder.Build(text);
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      Text = TextBuilder.Unbuild(atom, new System.Text.StringBuilder()).ToString();
      Error = null;
    }

    public string Text { get; }
    public TextAtom Atom { get; }
    public string Error { get; }
  }
}
