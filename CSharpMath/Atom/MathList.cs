using System;
using System.Collections.Generic;
using System.Collections;

namespace CSharpMath.Atom
 {
#pragma warning disable CA1710 // Identifiers should have correct suffix
  // WTF CA1710, you want types inheriting IList to have the Collection suffix?
  public class MathList : IMathObject, IList<MathAtom>, IReadOnlyList<MathAtom>, IEquatable<MathList> {
#pragma warning restore CA1710 // Identifiers should have correct suffix
    public List<MathAtom> Atoms { get; private set; }
    public MathList() => Atoms = new List<MathAtom>();
    public MathList(IEnumerable<MathAtom> atoms) => Atoms = new List<MathAtom>(atoms);
    public MathList(params MathAtom[] atoms) => Atoms = new List<MathAtom>(atoms);
    /// <summary>Just a deep copy if finalize is false; A finalized list if finalize is true</summary>
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
            case (null, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.BinaryOperator _, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.Relation _, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.Open _, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.Punctuation _, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.LargeOperator _, Atoms.BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (Atoms.BinaryOperator b, Atoms.Relation _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atoms.BinaryOperator b, Atoms.Punctuation _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atoms.BinaryOperator b, Atoms.Close _):
              newList[newList.Count - 1] = b.ToUnaryOperator();
              break;
            case (Atoms.Number { Subscript: null, Superscript: null } n, Atoms.Number _):
              n.Fuse(newNode);
              continue; // do not add the new node; we fused it instead.
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
    public void RemoveAtoms(int index, int count) => Atoms.RemoveRange(index, count);
    public bool EqualsList(MathList otherList) {
      if (otherList == null) {
        return false;
      } 
      if (otherList.Count != Count) {
        return false;
      }
      for (int i=0; i < Count; i++) {
        if (!this[i].NullCheckingStructuralEquality(otherList[i])) {
          return false;
        }
      }
      return true;
    }
    public override bool Equals(object obj) => obj is MathList l ? EqualsList(l) : false;
    public override int GetHashCode() =>
      Atoms.Count == 0 ? 0 : Atoms.GetHashCode(); // Special case empty list for LaTeXDefaults
    bool IEquatable<MathList>.Equals(MathList otherList) => EqualsList(otherList);
    public IEnumerator<MathAtom> GetEnumerator() => Atoms.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Atoms.GetEnumerator();
    public int IndexOf(MathAtom item) => Atoms.IndexOf(item);
    public void Insert(int index, MathAtom item) {
      if (item != null) Atoms.Insert(index, item);
      else throw new ArgumentNullException(nameof(item), "MathList cannot contain null.");
    }
    public void RemoveAt(int index) => Atoms.RemoveAt(index);
    public void Add(MathAtom item) {
      if (item != null) Atoms.Add(item);
      else throw new ArgumentNullException(nameof(item), "MathList cannot contain null.");
    }
    public void Clear() => Atoms.Clear();
    public bool Contains(MathAtom item) => Atoms.Contains(item);
    public void CopyTo(MathAtom[] array, int arrayIndex) => Atoms.CopyTo(array, arrayIndex);
    public bool Remove(MathAtom item) => Atoms.Remove(item);
    public MathList Slice(int index, int count) => new MathList { Atoms = Atoms.GetRange(index, count) };
  }
}