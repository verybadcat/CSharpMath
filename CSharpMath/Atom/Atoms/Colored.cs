using System.Text;
using System.Drawing;

namespace CSharpMath.Atom.Atoms {
  public sealed class Colored : MathAtom, IMathListContainer {
    public Color Color { get; set; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public Colored(Color color, MathList innerList) : base(string.Empty) =>
      (Color, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\color")
      .AppendInBracesOrLiteralNull(Color.ToString())
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new Colored Clone(bool finalize) => (Colored)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Colored(Color, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), Color, InnerList).GetHashCode();
  }
}