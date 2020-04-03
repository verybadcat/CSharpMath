using System.Text;

namespace CSharpMath.Atom.Atoms {
  public sealed class ColorBox : MathAtom, IMathListContainer {
    public Structures.Color Colour { get; set; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public ColorBox(Structures.Color color, MathList innerList) : base(string.Empty) =>
      (Colour, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\colorbox")
      .AppendInBracesOrLiteralNull(Colour.ToString())
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new ColorBox Clone(bool finalize) => (ColorBox)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new ColorBox(Colour, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), Colour, InnerList).GetHashCode();
  }
}