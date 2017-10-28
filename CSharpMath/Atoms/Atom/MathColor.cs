using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathColor: MathAtom, IMathColor {
    public string ColorString { get; set; }
    public IMathList InnerList { get; set; }
    public MathColor(): base(MathAtomType.Color, "") { }

    public override string StringValue {
      get {
        var builder = new StringBuilder(@"\color");
        builder.AppendInBraces(ColorString, NullHandling.LiteralNull);
        builder.AppendInBraces(InnerList, NullHandling.LiteralNull);
        return builder.ToString();
      }
    }
    public MathColor(MathColor cloneMe, bool finalize): base(cloneMe, finalize) {
      InnerList = AtomCloner.Clone(cloneMe.InnerList, finalize);
    }
  }
}
