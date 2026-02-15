namespace CSharpMath.Atom.Atoms {
  public sealed class Space : MathAtom {
    private readonly Length _space;
    public float Length => _space.Amount;
    public bool IsMu => _space.IsMu;
    public Space(Length space) : base() =>
      _space = space;
    public override bool ScriptsAllowed => false;
    public new Space Clone(bool finalize) => (Space)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Space(_space);
    public float ActualLength<TFont, TGlyph>
      (Display.FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.FrontEnd.IFont<TGlyph> => _space.ActualLength(mathTable, font);
    public override string DebugString => " ";
    public override bool Equals(object obj) => obj is Space s && EqualsSpace(s);
    public bool EqualsSpace(Space otherSpace) =>
      EqualsAtom(otherSpace) && Length == otherSpace.Length && IsMu == otherSpace.IsMu;
    public override int GetHashCode() => (base.GetHashCode(), _space).GetHashCode();
  }
}