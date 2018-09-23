namespace CSharpMath.Rendering {
  
  public readonly struct TextSource : ISource {
    public TextSource(string latex) {
      LaTeX = latex;
      var result = TextBuilder.Build(System.MemoryExtensions.AsSpan(latex));
      if (result.Error != null) {
        ErrorMessage = result.Error;
        Atom = null;
      } else {
        Atom = result.Value;
        ErrorMessage = null;
      }
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      LaTeX = TextBuilder.Unbuild(atom, new System.Text.StringBuilder()).ToString();
      ErrorMessage = null;
    }

    public string LaTeX { get; }
    public TextAtom Atom { get; }
    public string ErrorMessage { get; }
    public bool IsValid => Atom != null;
  }
}
