namespace CSharpMath.Rendering {
  public readonly struct TextSource : ISource {
    public static bool EnhancedColors { get; set; } = true;

    public TextSource(string text) {
      Text = text;
      (Atom, ErrorMessage) = TextBuilder.Build(text, EnhancedColors);
    }
    public TextSource(TextAtom atom) {
      Atom = atom;
      Text = TextBuilder.Unbuild(atom, new System.Text.StringBuilder()).ToString();
      ErrorMessage = null;
    }

    public string Text { get; }
    public TextAtom Atom { get; }
    public string ErrorMessage { get; }
    public bool IsValid => Atom != null;
  }
}
