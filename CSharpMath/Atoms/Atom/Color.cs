using System.Text;

namespace CSharpMath.Atoms.Atom {
  public class Color : MathAtom {
    public string ColorString { get; set; }
    public MathList InnerList { get; set; }
    public Color(string color, MathList innerList) : base(string.Empty) =>
      (ColorString, InnerList) = (color, innerList);
    public override string DebugString =>
      new StringBuilder(@"\color")
      .AppendInBracesOrLiteralNull(ColorString)
      .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new Color Clone(bool finalize) => (Color)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Color(ColorString, InnerList.Clone(finalize));
  }
}