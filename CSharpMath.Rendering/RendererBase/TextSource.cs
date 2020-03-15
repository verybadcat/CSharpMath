namespace CSharpMath.Rendering.Renderer {
  using Text;
  public class TextSource : ISource {
    public static TextSource FromLaTeX(string latex) {
      var (textAtom, errorMessage) = TextLaTeXBuilder.TextAtomFromLaTeX(latex);
      return new TextSource(textAtom) { ErrorMessage = errorMessage, _latex = latex };
    }
    public TextSource(TextAtom atom) => Atom = atom;
    private string _latex;
    public string LaTeX =>
      _latex ??= TextLaTeXBuilder.TextAtomToLaTeX(Atom, new System.Text.StringBuilder()).ToString();
    public TextAtom Atom { get; }
    public string ErrorMessage { get; private set; }
    public bool IsValid => Atom != null;
  }
}
