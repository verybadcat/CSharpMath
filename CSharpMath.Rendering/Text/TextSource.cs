namespace CSharpMath.Rendering {
  [System.Obsolete("The Text classes are not yet usable in this prerelease.", true)]
  public readonly struct TextSource : ISource {
    public TextSource(string text) {
      Text = text;
      (Atom, ErrorMessage) = TextBuilder.Build(text);
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      Text = TextBuilder.Unbuild(atom, new System.Text.StringBuilder()).ToString();
      ErrorMessage = null;
    }

    public string Text { get; }
    public TextAtom Atom { get; }
    public string ErrorMessage { get; }
    public bool IsEmpty => Atom == null;
  }
}
