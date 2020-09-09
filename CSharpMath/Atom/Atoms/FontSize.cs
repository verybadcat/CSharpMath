using System.Text;

namespace CSharpMath.Atom.Atoms {
  public sealed class FontSize : MathAtom {
    // https://github.com/mathjax/MathJax-src/blob/32213009962a887e262d9930adcfb468da4967ce/ts/input/tex/textmacros/TextMacrosMappings.ts#L90-L99
    public enum SizePercentage {
      tiny = 50,
      scriptsize = 70,
      small = 85,
      normalsize = 100,
      large = 120,
      Large = 144,
      LARGE = 173,
      huge = 207,
      Huge = 249
    }
    public SizePercentage Size { get; }
    public FontSize(SizePercentage size) : base(string.Empty) => Size = size;
    public override string DebugString =>
      new StringBuilder(@"\fontsize")
      .AppendInBracesOrLiteralNull(Size.ToStringInvariant()).ToString();
    public override bool ScriptsAllowed => false;
    public new FontSize Clone(bool finalize) => (FontSize)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new FontSize(Size);
    public override int GetHashCode() => (base.GetHashCode(), Size).GetHashCode();
  }
}