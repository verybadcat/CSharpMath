using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;

namespace CSharpMath.Atoms
{
  public class MathStyle : MathAtom, IMathStyle {
    private LineStyle _style;

    public MathStyle(LineStyle style): base(MathAtomType.Style, "") {
      _style = style;
    }
    public MathStyle(MathStyle cloneMe, bool finalize): base(cloneMe, finalize) {
      _style = cloneMe.Style;
    }
    public LineStyle Style => _style;

    public bool EqualsStyle(MathStyle otherStyle) {
      bool r = EqualsAtom(otherStyle);
      r &= Style == otherStyle.Style;
      return r;
    }

    public override bool Equals(object obj)
      => EqualsStyle(obj as MathStyle);

    public override int GetHashCode() {
      unchecked {
        return base.GetHashCode() + 107 * _style.GetHashCode();
      }
    }
  }
}
