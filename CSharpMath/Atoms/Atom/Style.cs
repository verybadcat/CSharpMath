using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms.Atom {
  ///<summary>Style changes during rendering</summary>
  public class Style : MathAtom {
    public Style(LineStyle style) : base(MathAtomType.Style, string.Empty) => LineStyle = style;
    public LineStyle LineStyle { get; }
    public override bool ScriptsAllowed => false;
    public new Style Clone(bool finalize) => (Style)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Style(LineStyle);
    public override string DebugString => @"\" + LineStyle.ToString().ToLowerInvariant() + "style";
    public bool EqualsStyle(Style otherStyle) => EqualsAtom(otherStyle) && LineStyle == otherStyle.LineStyle;
    public override bool Equals(object obj) => obj is Style s ? EqualsStyle(s) : false;
    public override int GetHashCode() => unchecked(base.GetHashCode() + 107 * LineStyle.GetHashCode());
  }
}
