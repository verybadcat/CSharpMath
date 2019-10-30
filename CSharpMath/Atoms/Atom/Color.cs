using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class Color: MathAtom, IColor {
    public string ColorString { get; set; }
    public IMathList InnerList { get; set; }
    public Color(): base(MathAtomType.Color, string.Empty, string.Empty) { }

    public override string StringValue {
      get {
        var builder = new StringBuilder(@"\color");
        builder.AppendInBraces(ColorString, NullHandling.LiteralNull);
        builder.AppendInBraces(InnerList, NullHandling.LiteralNull);
        return builder.ToString();
      }
    }
    public Color(Color cloneMe, bool finalize): base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
      ColorString = cloneMe.ColorString;
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
       => visitor.Visit(this, helper);
  }
}
