using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms {
  public class Radical: MathAtom, IRadical {
    public IMathList Degree { get; set; }
    public IMathList Radicand { get; set; }
    public Radical(): base(MathAtomType.Radical, string.Empty, string.Empty) {

    }

    

    public Radical(MathAtomType type, string value): this() {
      
    }

    public Radical(Radical cloneMe, bool finalize): base(cloneMe, finalize) {
      Radicand = AtomCloner.Clone(cloneMe.Radicand, finalize);
      Degree = AtomCloner.Clone(cloneMe.Degree, finalize);
    }

    public override string StringValue {
      get {
        var builder = new StringBuilder(@"\Sqrt");
        builder.AppendInSquareBrackets(Degree?.StringValue, NullHandling.EmptyString);
        builder.AppendInBraces(Radicand.StringValue, NullHandling.LiteralNull);
        builder.AppendScripts(this);
        return builder.ToString();
      }
    }

    public override T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
=> visitor.Visit(this, helper);
  }
}
