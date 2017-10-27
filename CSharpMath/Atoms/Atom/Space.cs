using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathSpace : MathAtom, ISpace {
    private readonly float _space; // mu units
    public float Space => _space;

    public MathSpace(float space): base(MathAtomType.Space, "") {
      _space = space;
    }

   
  }
}
