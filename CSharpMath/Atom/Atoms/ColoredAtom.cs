using System.Text;
using System.Drawing;

namespace CSharpMath.Atom.Atoms {
  public sealed class ColoredAtom : MathAtom, IMathListContainer {
    public Color Colour { get; set; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public ColoredAtom(Color color, MathList innerList) : base(string.Empty) =>
      (Colour, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\color")
      .AppendInBracesOrLiteralNull(Colour.ToString())
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new ColoredAtom Clone(bool finalize) => (ColoredAtom)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new ColoredAtom(Colour, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), Colour, InnerList).GetHashCode();
  }
}