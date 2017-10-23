using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Fraction : MathAtom, IFraction {
    public IMathList Numerator { get; set; }
    public IMathList Denominator { get; set; }
    public string LeftDelimiter { get; set; }
    public string RightDelimiter { get; set; }

    public bool HasRule { get; private set; }

    public Fraction(bool hasRule): base(MathAtomType.Fraction, "") {

    }

    public override string StringValue {
      get {
        var builder = new StringBuilder();
        if (HasRule) {
          builder.Append(@"\atop");
        } else {
          builder.Append(@"\frac");
        }
        if (LeftDelimiter!=null || RightDelimiter!=null) {
          builder.Append($@"[{LeftDelimiter.NullToNull(NullHandling.EmptyContent)}][{RightDelimiter.NullToNull(NullHandling.EmptyContent)}]");
        }
        builder.AppendInBraces(Numerator.StringValue, NullHandling.EmptyString);
        builder.AppendInBraces(Denominator.StringValue, NullHandling.EmptyString);
        builder.AppendScripts(this);
        return builder.ToString();
      }
    }
    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
  => visitor.Visit(this, helper);
  }
}
