using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace CSharpMath.Atoms {
  public class MathList : IMathList {

    public List<IMathAtom> Atoms { get; set; } = new List<IMathAtom>();
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

    public MathList() { }

    public bool IsAtomAllowed(IMathAtom atom) {
      return atom != null && atom.AtomType != MathAtomType.Boundary;
    }

    public MathList(IMathList cloneMe, bool finalize) : this() {
      if (!finalize) {
        foreach (var atom in cloneMe.Atoms) {
          var cloneAtom = AtomCloner.Clone(atom, finalize);
          Add(cloneAtom);

        }
      } else {
        IMathAtom prevNode = null;
        foreach (IMathAtom atom in cloneMe.Atoms) {
          var newNode = AtomCloner.Clone(atom, finalize);
          if (atom.IndexRange == Ranges.Zero) {
            int prevIndex = (prevNode == null) ? 0 : prevNode.IndexRange.Location + prevNode.IndexRange.Length;
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
              if (prevNode != null && prevNode.AtomType == MathAtomType.BinaryOperator) {
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
          Add(newNode);
          prevNode = newNode;

        }
      }

    }

    public int Count => Atoms.Count;

    public string StringValue {
      get {
        var builder = new StringBuilder();
        foreach (var atom in Atoms) {
          builder.Append(atom.StringValue);
        }
        return builder.ToString();
      }
    }

    public bool IsReadOnly => false;

    public IMathAtom this[int index] { get => Atoms[index]; set => Atoms[index] = value; }

    public void Append(IMathList list) => this.Atoms.AddRange(list.Atoms);
    public IMathList DeepCopy() => AtomCloner.Clone(this, false);
    public IMathList FinalizedList() => AtomCloner.Clone(this, true);
    public void RemoveAtoms(Range inRange) => Atoms.RemoveRange(inRange.Location, inRange.Length);
    public void RemoveLastAtom() {
      if (Atoms.Count > 0) {
        Atoms.RemoveAt(Atoms.Count - 1);
      }
    }
    public bool EqualsList(MathList otherList) {
      if (otherList == null) {
        return false;
      } 
      if (otherList.Count!=this.Count) {
        return false;
      }
      for (int i=0; i<this.Count; i++) {
        if (!this[i].NullCheckingEquals(otherList[i])) {
          return false;
        }
      }
      return true;
    }
    public override bool Equals(object obj) {
      if (obj is MathList) {
        return EqualsList((MathList)obj);
      }
      return false;
    }
    public override int GetHashCode() => Atoms.GetHashCode();
    public IEnumerator<IMathAtom> GetEnumerator() => Atoms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Atoms.GetEnumerator();
    public int IndexOf(IMathAtom item) => Atoms.IndexOf(item);
    private void ThrowInvalid(IMathAtom item) {
      if (item == null) {
        throw new InvalidOperationException("MathList cannot contain null.");
      }
      if (item.AtomType == MathAtomType.Boundary) {
        throw new InvalidOperationException("MathList cannot contain items of type " + item.AtomType);
      }
    }
    public void Insert(int index, IMathAtom item) {
      if (IsAtomAllowed(item)) {
        Atoms.Insert(index, item);
      } else {
        ThrowInvalid(item);
      }
    }
    public void RemoveAt(int index) => Atoms.RemoveAt(index);
    public void Add(IMathAtom item) {
      if (IsAtomAllowed(item)) {
        Atoms.Add(item);
      } else {
        ThrowInvalid(item);
      }
    }
    public void Clear() => Atoms.Clear();
    public bool Contains(IMathAtom item) => Atoms.Contains(item);
    public void CopyTo(IMathAtom[] array, int arrayIndex) => Atoms.CopyTo(array, arrayIndex);
    public bool Remove(IMathAtom item) => Atoms.Remove(item);
  }
}
