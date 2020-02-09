using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms.Atom {
  /// <summary>An overlined atom</summary>
  public class Overline : MathAtom {
    public MathList InnerList { get; set; }
    public Overline(MathList innerList) : base(MathAtomType.Overline, string.Empty) =>
      InnerList = innerList;
    public override bool ScriptsAllowed => true;
    public new Overline Clone(bool finalize) => (Overline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Overline(InnerList.Clone(finalize));
    public override string DebugString =>
      new StringBuilder(@"\overline")
      .AppendInBraces(InnerList.DebugString, NullHandling.LiteralNull)
      .ToString();
    public bool EqualsOverline(Overline? other) =>
      EqualsAtom(other) && InnerList.NullCheckingEquals(other?.InnerList);
    public override bool Equals(object obj) => EqualsOverline(obj as Overline);
    public override int GetHashCode() =>
      unchecked(base.GetHashCode() + 53 * InnerList.GetHashCode());
  }
}
