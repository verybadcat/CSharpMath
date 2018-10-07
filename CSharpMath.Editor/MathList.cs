using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  class MathList : IMathList {
    void InsertAtom(MathAtom atom, MathListIndex index) {
      if (index.AtomIndex > Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {Atoms.Count}");
      switch (index.SubIndexType) {
        case MathListSubIndexType.None:
          InsertAtom(atom, index.AtomIndex);
          break;
      }
    }
  }
}
