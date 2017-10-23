using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  class MathList : IMathList {

    private static bool IsNotBinaryOperator(IMathAtom prevNode) {
      if (prevNode == null) {
        return true;
      }
      switch (prevNode.AtomType) {
        case MathAtomType.BinaryOperator:
        case MathAtomType.Relation:
        case MathAtomType.Open:
        case MathAtomType.Punctuation:
        case MathAtomType.LargeOperator:
          return true;
        default:
          return false;
      }
    }
    public List<IMathAtom> Atoms { get; set; } = new List<IMathAtom>();

    public string StringValue => throw new NotImplementedException();

    public void AddAtom(IMathAtom atom) => throw new NotImplementedException();
    public void Append(IMathList list) => throw new NotImplementedException();
    public IMathList DeepCopy() => throw new NotImplementedException();
    public IMathList FinalizedList() => throw new NotImplementedException();
    public void InsertAtom(IMathAtom atom, int index) => throw new NotImplementedException();
    public void RemoveAtom(int index) => throw new NotImplementedException();
    public void RemoveAtoms(Range inRange) => throw new NotImplementedException();
    public void RemoveLastAtom() => throw new NotImplementedException();
    public MathList() { }
    public MathList(MathList cloneMe, bool finalize): this() {
      if (!finalize) {
        foreach(var atom in cloneMe.Atoms) {
          var cloneAtom = AtomCloner.Clone(atom, finalize);
          AddAtom(cloneAtom);
          
        }
      } else {
        IMathAtom prevNode = null;
        foreach (IMathAtom atom in Atoms) {
          var newNode = AtomCloner.Clone(atom, finalize);
          if (atom.IndexRange == Ranges.Zero) {
            int prevIndex = (prevNode == null) ? 0 : prevNode.IndexRange.End;
            newNode.IndexRange = new Range(prevIndex, 1);
          }
          switch (newNode.AtomType) {
            case MathAtomType.BinaryOperator:
              if (IsNotBinaryOperator(prevNode)) {
                newNode.AtomType = MathAtomType.UnaryOperator;
              }
              break;
            case MathAtomType.Relation:
            case MathAtomType.Punctuation:
            case MathAtomType.Close:
              if (prevNode!=null && prevNode.AtomType == MathAtomType.BinaryOperator) {
                prevNode.AtomType = MathAtomType.UnaryOperator;
              }
              break;
            case MathAtomType.Number:
              if (prevNode != null && prevNode.AtomType == MathAtomType.Number && prevNode.Subscript == null && prevNode.Superscript == null) {
                prevNode.Fuse(newNode);
                continue; // do not add the new node; we fused it instead.
              }
              break;
            
          }
          AddAtom(newNode);
          prevNode = newNode;
          
        }
      }
    }
  }
}
