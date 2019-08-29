using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class LargeOperator : MathAtom {
    bool? _limits;
    /// <summary>
    /// True: \limits
    /// False: \nolimits
    /// Null: (unset, depends on line style)
    /// </summary>
    public bool? Limits { get => NoLimits ? false : _limits; set => _limits = value; }

    ///<summary>If true, overrides Limits and makes it treated as false</summary>
    public bool NoLimits { get; }

    public LargeOperator(string value, bool? limits, bool noLimits = false): base(MathAtomType.LargeOperator, value) {
      Limits = limits;
      NoLimits = noLimits;
    }

    public LargeOperator(LargeOperator cloneMe, bool finalize): base(cloneMe, finalize) {
      NoLimits = cloneMe.NoLimits;
      _limits = cloneMe._limits;
    }

    public override string StringValue => base.StringValue + (Limits == true ? @"\limits" : Limits == false && !NoLimits ? @"\nolimits" : string.Empty);

    public bool EqualsLargeOperator(LargeOperator obj) =>
      EqualsAtom(obj)
      && Limits == obj.Limits;

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);
  }
}
