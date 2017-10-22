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
      switch (prevNode.ItemType) {
        case MathItemType.BinaryOperator:
        case MathItemType.Relation:
        case MathItemType.Open:
        case MathItemType.Punctuation:
        case MathItemType.LargeOperator:
          return true;
        default:
          return false;
      }
    }
    public List<IMathAtom> Atoms => throw new NotImplementedException();

    public string StringValue => throw new NotImplementedException();

    public void AddAtom(IMathAtom atom) => throw new NotImplementedException();
    public void Append(IMathList list) => throw new NotImplementedException();
    public IMathList DeepCopy() => throw new NotImplementedException();
    public IMathList FinalizedList() => throw new NotImplementedException();
    public void InsertAtom(IMathAtom atom, int index) => throw new NotImplementedException();
    public void RemoveAtom(int index) => throw new NotImplementedException();
    public void RemoveAtoms(Range inRange) => throw new NotImplementedException();
    public void RemoveLastAtom() => throw new NotImplementedException();
  }
}
