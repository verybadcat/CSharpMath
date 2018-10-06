namespace CSharpMath.Editor {
  using Atoms;
  interface IMathList {

     void InsertAtom(MathAtom atom, MathListIndex index);

     void RemoveAtomAtListIndex(MathListIndex index);

     void RemoveAtomsInListIndexRange(Range range);

     // Get the atom at the given index. If there is none, or index is invalid returns nil.
     MathAtom AtomAtListIndex(int index);
  }
}
