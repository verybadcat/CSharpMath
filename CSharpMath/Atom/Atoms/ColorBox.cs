using System.Drawing;
using System.Text;

namespace CSharpMath.Atom.Atoms {
  public sealed class ColorBox : MathAtom, IMathListContainer {
    public Color Color { get; set; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public ColorBox(Color color, MathList innerList) : base(string.Empty) =>
      (Color, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\colorbox")
      .AppendInBracesOrLiteralNull(Color.ToString())
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new ColorBox Clone(bool finalize) => (ColorBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new ColorBox(Color, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), Color, InnerList).GetHashCode();
  }
}