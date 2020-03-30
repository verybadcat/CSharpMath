namespace CSharpMath.Atom.Atoms {
  public sealed class Prime : MathAtom {
    public Prime(int length) : base(PrimeOfLength(length)) =>
      Length = length;
    public int Length { get; }
    public override bool ScriptsAllowed => true;
    public new Prime Clone(bool finalize) => (Prime)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Prime(Length);
    private static string PrimeOfLength(int length) {
      if (length <= 0)
        throw new System.ArgumentOutOfRangeException(
          nameof(length), length, "Only positive length is allowed.");
      var sb = new System.Text.StringBuilder();
      Append: switch (length) {
        //glyphs are already superscripted
        //pick appropriate codepoint depending on number of primes
        case 1: sb.Append('\u2032'); break;
        case 2: sb.Append('\u2033'); break;
        case 3: sb.Append('\u2034'); break;
        case 4: sb.Append('\u2057'); break;
        default: sb.Append('\u2057'); length -= 4; goto Append;
      }
      return sb.ToString();
    }
    public override int GetHashCode() => unchecked(base.GetHashCode() + 401 * Length);
    public bool EqualsPrime(Prime other) => EqualsAtom(other) && Length == other.Length;
    public override bool Equals(object obj) => obj is Prime p ? EqualsPrime(p) : false; 
  }
}