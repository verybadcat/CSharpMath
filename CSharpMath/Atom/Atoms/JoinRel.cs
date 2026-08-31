namespace CSharpMath.Atom.Atoms {
  /// <summary>The plain-TeX relation joiner, which contributes -3mu.</summary>
  public sealed class JoinRel : MathAtom {
    public JoinRel() : base() { }
    // Plain TeX's joiner is an invisible spacing atom. Scripts are rejected by
    // the parser rather than being silently discarded by the typesetter.
    public override bool ScriptsAllowed => true;
    public new JoinRel Clone(bool finalize) => (JoinRel)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new JoinRel();
    public override string DebugString => @"\joinrel" +
      new System.Text.StringBuilder().AppendDebugStringOfScripts(this).ToString();
  }
}
