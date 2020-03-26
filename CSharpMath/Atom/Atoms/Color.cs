using System.Text;

namespace CSharpMath.Atom.Atoms {
  public class Color : MathAtom, IMathListContainer {
    public Structures.Color Colour { get; set; }
    public MathList InnerList { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public Color(Structures.Color color, MathList innerList) : base(string.Empty) =>
      (Colour, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\color")
      .AppendInBracesOrLiteralNull(Colour.ToString())
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new Color Clone(bool finalize) => (Color)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Color(Colour, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), InnerList).GetHashCode();
  }
}