using System;
using System.Collections.Generic;
using System.Collections;

namespace CSharpMath.Atoms {
  public class MathList : IMathObject, IList<MathAtom> {
    public List<MathAtom> Atoms { get; set; }
    public MathList() => Atoms = new List<MathAtom>();
    public MathList(IEnumerable<MathAtom> atoms) => Atoms = new List<MathAtom>(atoms);
    public MathList(params MathAtom[] atoms) => Atoms = new List<MathAtom>(atoms);
    public bool IsAtomAllowed(MathAtom atom) => atom != null;
    public MathList Clone(bool finalize) {
      var newList = new MathList();
      if (!finalize) {
        foreach (var atom in Atoms)
          newList.Add(atom.Clone(finalize));
      } else {
        foreach (var atom in Atoms) {
          var prevNode = newList.Count > 0 ? newList[newList.Count - 1] : null;
          var newNode = atom.Clone(finalize);
          if (atom.IndexRange == Range.Zero) {
            int prevIndex =
              prevNode?.IndexRange.Location + prevNode?.IndexRange.Length ?? 0;
            newNode.IndexRange = new Range(prevIndex, 1);
          }
#warning One day when C# receives "or patterns", simplify this abomination
          switch (prevNode, newNode) {
            case (null, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.BinaryOperator _, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.Relation _, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.Open _, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.Punctuation _, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.LargeOperator _, Atom.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atom.BinaryOperator b, Atom.Relation _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atom.BinaryOperator b, Atom.Punctuation _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atom.BinaryOperator b, Atom.Close _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atom.Number { Subscript: null, Superscript: null } n, Atom.Number _):
              n.Fuse(newNode); // do not add the new node; we fused it instead.
              continue;
          }
          newList.Add(newNode);
        }
      }
      return newList;
    }
    public int Count => Atoms.Count;
    public string DebugString =>
      string.Concat(System.Linq.Enumerable.Select(Atoms, a => a.DebugString));
    public bool IsReadOnly => false;
    public MathAtom this[int index] { get => Atoms[index]; set => Atoms[index] = value; }
    public void Append(IEnumerable<MathAtom> list) => Atoms.AddRange(list);
    public MathList DeepCopy() => Clone(false);
    public MathList FinalizedList() => Clone(true);
    public void RemoveAtoms(Range inRange) => Atoms.RemoveRange(inRange.Location, inRange.Length);
    public bool EqualsList(MathList otherList) {
      if (otherList == null) {
        return false;
      } 
      if (otherList.Count != Count) {
        return false;
      }
      for (int i=0; i < Count; i++) {
        if (!this[i].NullCheckingEquals(otherList[i])) {
          return false;
        }
      }
      return true;
    }
    public override bool Equals(object obj) => obj is MathList l ? EqualsList(l) : false;
    public override int GetHashCode() => Atoms.GetHashCode();
    public IEnumerator<MathAtom> GetEnumerator() => Atoms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Atoms.GetEnumerator();
    public int IndexOf(MathAtom item) => Atoms.IndexOf(item);
    private void ThrowInvalid(MathAtom item) {
      if (item is null) {
        throw new ArgumentNullException(nameof(item), "MathList cannot contain null.");
      }
    }
    public void Insert(int index, MathAtom item) {
      if (IsAtomAllowed(item)) Atoms.Insert(index, item); else ThrowInvalid(item);
    }
    public void RemoveAt(int index) => Atoms.RemoveAt(index);
    public void Add(MathAtom item) {
      if (IsAtomAllowed(item)) Atoms.Add(item);
      else ThrowInvalid(item);
    }
    public void Clear() => Atoms.Clear();
    public bool Contains(MathAtom item) => Atoms.Contains(item);
    public void CopyTo(MathAtom[] array, int arrayIndex) => Atoms.CopyTo(array, arrayIndex);
    public bool Remove(MathAtom item) => Atoms.Remove(item);
    public MathList GetRange(int index, int count) =>
      new MathList { Atoms = Atoms.GetRange(index, count) };
  }
}
