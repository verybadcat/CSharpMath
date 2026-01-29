using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharpMath.Atom {
  using Atoms;
#pragma warning disable CA1710 // Identifiers should have correct suffix
  // WTF CA1710, you want types inheriting IList to have the Collection suffix?
  class DisabledMathList : MathList {
    internal DisabledMathList() { }
    public override void Add(MathAtom item) => throw new InvalidOperationException("Scripts are not allowed!");
    public override void Append(IEnumerable<MathAtom> list) => throw new InvalidOperationException("Scripts are not allowed!");
  }
  public class MathList : IMathObject, IList<MathAtom>, IReadOnlyList<MathAtom>, IEquatable<MathList> {
#pragma warning restore CA1710 // Identifiers should have correct suffix
    public List<MathAtom> Atoms { get; private set; }
    public MathList() => Atoms = new List<MathAtom>();
    public MathList(IEnumerable<MathAtom> atoms) => Atoms = new List<MathAtom>(atoms);
    public MathList(params MathAtom[] atoms) => Atoms = new List<MathAtom>(atoms);

    /// <returns>The last <see cref="MathAtom"/> that is not a <see cref="Comment"/>,
    /// or <see cref="null"/> when <see cref="Atoms"/> is empty.</returns>
#if !NETSTANDARD2_0 && !NET45
    [System.Diagnostics.CodeAnalysis.DisallowNull]
#endif
    public MathAtom? Last {
      get {
        for (int i = Atoms.Count - 1; i >= 0; i--)
          switch (Atoms[i]) {
            case Comment _:
              continue;
            case var atom:
              return atom;
          }
        return null;
      }
      set {
        for (int i = Atoms.Count - 1; i >= 0; i--)
          switch (Atoms[i]) {
            case Comment _:
              continue;
            default:
              Atoms[i] = value!;
              return;
          }
        Atoms.Add(value!);
      }
    }
    /// <summary>Just a deep copy if finalize is false; A finalized list if finalize is true</summary>
    public MathList Clone(bool finalize) {
      var newList = new MathList();
      if (!finalize) {
        foreach (var atom in Atoms)
          newList.Add(atom.Clone(finalize));
      } else {
        foreach (var atom in Atoms) {
          if (atom is Comment) {
            var newComment = atom.Clone(finalize);
            newComment.IndexRange = Range.NotFound;
            newList.Add(newComment);
            continue;
          }
          var prevNode = newList.Last;
          var newNode = atom.Clone(finalize);
          if (atom.IndexRange == Range.Zero) {
            int prevIndex =
              prevNode?.IndexRange.Location + prevNode?.IndexRange.Length ?? 0;
            newNode.IndexRange = new Range(prevIndex, 1);
          }
          switch (prevNode, newNode) {
            case (null or BinaryOperator or Relation or Open or Punctuation or LargeOperator, BinaryOperator b):
              newNode = b.ToUnaryOperator();
              break;
            case (BinaryOperator b, Relation or Punctuation or Close):
              newList.Last = b.ToUnaryOperator();
              break;
            case (Number n, Number _) when n.Superscript.IsEmpty() && n.Subscript.IsEmpty():
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
    public virtual void Append(IEnumerable<MathAtom> list) => Atoms.AddRange(list);
    public void RemoveAtoms(int index, int count) => Atoms.RemoveRange(index, count);
    public bool EqualsList(MathList otherList) {
      if (otherList == null) {
        return false;
      }
      if (otherList.Count != Count) {
        return false;
      }
      for (int i = 0; i < Count; i++) {
        if (!this[i].NullCheckingStructuralEquality(otherList[i])) {
          return false;
        }
      }
      return true;
    }
    public override bool Equals(object obj) => obj is MathList l && EqualsList(l);
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
    public virtual void Add(MathAtom item) {
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