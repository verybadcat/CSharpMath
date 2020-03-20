using System.Text;

namespace CSharpMath.Atom.Atoms {
  public class Color : MathAtom, IMathListContainer1 {
    public Structures.Color Colour { get; set; }
    public MathList InnerList { get; }
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