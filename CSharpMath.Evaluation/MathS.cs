using System;
using System.Collections.Generic;
using AngouriMath;

namespace CSharpMath.Evaluation {
  using Atom;
  using Atoms = Atom.Atoms;
  public class MathS {
    public Structures.Result<Entity> ToMathSEntity(MathList mathList) {
      mathList = mathList.Clone(true);
        try {
              return $"wip";
        } catch (MathSException e) {
          return e.Message;
        }
    }
  }
}
