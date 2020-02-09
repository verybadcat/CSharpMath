namespace CSharpMath.Atoms.Atom {
  /// <summary>Close brackets</summary>
  public class Close : MathAtom {
    public Close(string nucleus, bool hasCorrespondingOpen = true) : base(nucleus) =>
      HasCorrespondingOpen = hasCorrespondingOpen;
    public bool HasCorrespondingOpen { get; }
    public override bool ScriptsAllowed => true;
    public new Close Clone(bool finalize) => (Close)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Close(Nucleus);
  }
}