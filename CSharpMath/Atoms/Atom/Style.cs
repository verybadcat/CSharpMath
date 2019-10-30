using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms {
  public class Style : MathAtom, IStyle {
    public Style(LineStyle style) : base(MathAtomType.Style, string.Empty, string.Empty) {
      LineStyle = style;
    }
    public Style(Style cloneMe, bool finalize) : base(cloneMe, finalize) {
      LineStyle = cloneMe.LineStyle;
    }
    public LineStyle LineStyle { get; }

    public override string StringValue => @"\" + LineStyle.ToString().ToLowerInvariant() + "style";

    public bool EqualsStyle(Style otherStyle) =>
      EqualsAtom(otherStyle) && LineStyle == otherStyle.LineStyle;

    public override bool Equals(object obj)
      => EqualsStyle(obj as Style);

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode() + 107 * LineStyle.GetHashCode();
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
=> visitor.Visit(this, helper);
  }
}
