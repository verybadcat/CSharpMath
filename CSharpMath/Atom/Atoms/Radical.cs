using System.Text;

namespace CSharpMath.Atom.Atoms {
  public class Radical: MathAtom, IMathListContainer2 {
    public MathList? Degree { get; set; }
    /// <summary>Whatever is under the square root sign</summary>
    public MathList? Radicand { get; set; }
    MathList? IMathListContainer2.InnerList1 { get => Degree; set => Degree = value; }
    MathList? IMathListContainer2.InnerList2 { get => Radicand; set => Radicand = value; }
    public Radical() : base(string.Empty) { }
    public new Radical Clone(bool finalize) => (Radical)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Radical { Degree = Degree?.Clone(finalize), Radicand = Radicand?.Clone(finalize) };
    public override bool ScriptsAllowed => true;
    public override string DebugString =>
      new StringBuilder(@"\sqrt")
      .AppendInBracketsOrNothing(Degree?.DebugString)
      .AppendInBracesOrLiteralNull(Radicand?.DebugString)
      .AppendDebugStringOfScripts(this).ToString();
  }
}