namespace CSharpMath.Rendering {
  public readonly struct TextSource : ISource {
    public TextSource(string latex) {
      LaTeX = latex;
      (Atom, ErrorMessage) = TextBuilder.TextAtomFromLaTeX(latex);
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      LaTeX = TextBuilder.TextAtomToLaTeX(atom, new System.Text.StringBuilder()).ToString();
      ErrorMessage = null;
    }
    public string LaTeX { get; }
    public TextAtom Atom { get; }
    public string ErrorMessage { get; }
    public bool IsValid => Atom != null;
  }
}
