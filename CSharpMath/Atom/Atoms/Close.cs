namespace CSharpMath.Atom.Atoms {
  /// <summary>Close brackets</summary>
  public sealed class Close : MathAtom {
    public Close(string nucleus, bool hasCorrespondingOpen = true) : base(nucleus) =>
      HasCorrespondingOpen = hasCorrespondingOpen;
#warning To remove when doing issue #82
    [System.Obsolete("Should be removed when doing issue #82")]
    public bool HasCorrespondingOpen { get; }
    public override bool ScriptsAllowed => true;
    public new Close Clone(bool finalize) => (Close)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Close(Nucleus);
  }
}