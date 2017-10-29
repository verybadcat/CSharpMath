using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathLists {
    public static MathList WithAtoms(List<IMathAtom> atoms) {
      var r = new MathList();
      foreach (var atom in atoms) {
        r.AddAtom(atom);
      }
      return r;
    }

    public static MathList WithAtoms(params IMathAtom[] atoms) {
      var r = new MathList();
      foreach (var atom in atoms) {
        r.AddAtom(atom);
      }
      return r;
    }
  }
}
