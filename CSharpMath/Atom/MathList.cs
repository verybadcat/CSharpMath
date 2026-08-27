using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Atom {
  using Atoms;
  using MacroAtom = Atoms.Macro;
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
    /// <summary>A new list with every top-level macro replaced by its raw expansion.
    /// Non-macro atoms are carried over by reference.</summary>
    internal MathList MacroExpand() => MacroAtom.ExpandMacros(this);
    public MathList() => Atoms = new List<MathAtom>();
    public MathList(IEnumerable<MathAtom> atoms) => Atoms = new List<MathAtom>(atoms);
    public MathList(params MathAtom[] atoms) => Atoms = new List<MathAtom>(atoms);

    private static Type EffectiveClass(MathAtom atom) => atom switch {
      Stack stack => stack.DisplayClassType,
      LargeDelimiter large => large.MathClass,
      BinaryOperator _ => typeof(BinaryOperator),
      Relation _ => typeof(Relation),
      Open _ => typeof(Open),
      Close _ => typeof(Close),
      Punctuation _ => typeof(Punctuation),
      LargeOperator _ => typeof(LargeOperator),
      _ => typeof(Ordinary)
    };
    private static bool BinaryCannotFollow(MathAtom? atom) {
      if (atom is null) return true;
      var mathClass = EffectiveClass(atom);
      return mathClass == typeof(BinaryOperator) || mathClass == typeof(Relation) ||
        mathClass == typeof(Open) || mathClass == typeof(Punctuation) ||
        mathClass == typeof(LargeOperator);
    }
    private static MathAtom DemoteBinary(MathAtom atom) => atom switch {
      BinaryOperator binary => binary.ToUnaryOperator(),
      Stack stack when stack.DisplayClassType == typeof(BinaryOperator) =>
        DemoteBinaryStack(stack),
      _ => throw new InvalidCodePathException("Only binary-class atoms can be demoted")
    };

    private static Stack DemoteBinaryStack(Stack stack) {
      stack.DisplayClassType = typeof(Ordinary);
      return stack;
    }

    /// <returns>The last <see cref="MathAtom"/> that is not a <see cref="Comment"/>,
    /// or <see langword="null"/> when <see cref="Atoms"/> is empty.</returns>
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
        // Macros are expanded first, so the reclassifying pass sees the flat stream
        // a macro stands for rather than the macro atom itself.
        if (Atoms.Any(a => a is MacroAtom))
          return MacroExpand().Clone(true);
        MathAtom? prevNode = null;
        int prevDisplayedIndex = -1;
        foreach (var atom in Atoms) {
          var newNode = atom.Clone(finalize);
          var location = newNode.IndexRange == Range.Zero
            ? (prevNode is { } prev ? prev.IndexRange.Location + prev.IndexRange.Length : 0)
            : newNode.IndexRange.Location;
          if (newNode.IndexRange == Range.Zero) {
            // A brace group spans its contents, so its length is the number of atoms
            // inside; every other atom occupies exactly one index slot.
            int length = 1;
            if (newNode is Group g) {
              // Renumber the group contents to their global indices, then the group
              // spans from its location to the end of the last inner atom.
              g.OffsetInnerRanges(location);
              length = g.InnerList.LastOrDefault()?.IndexRange.End - location ?? 1;
              if (length < 1) length = 1;
            }
            newNode.IndexRange = new Range(location, length);
          }
          var previousDisplayed = prevDisplayedIndex == -1 ? null : newList[prevDisplayedIndex];
          var newClass = EffectiveClass(newNode);
          // Stack atoms participate through DisplayClassType in the same TeX Bin-to-Ord
          // normalization as native BinaryOperator/Relation atoms.
          if (newClass == typeof(BinaryOperator) && BinaryCannotFollow(previousDisplayed)) {
            newNode = DemoteBinary(newNode);
          } else if (previousDisplayed is { } previous
            && EffectiveClass(previous) == typeof(BinaryOperator)
            && (newClass == typeof(Relation) || newClass == typeof(Punctuation) ||
                newClass == typeof(Close))) {
            newList[prevDisplayedIndex] = DemoteBinary(previous);
          }
          // Combine numbers together, but never across a font-style change: Fuse
          // keeps the first atom's FontStyle and would corrupt the other's.
          if ((prevNode, newNode) is (Number { Superscript.Count: 0, Subscript.Count: 0 } n, Number)
            && n.FontStyle == newNode.FontStyle) {
            n.Fuse(newNode);
            continue; // do not add the new node; we fused it instead.
          }
          if (newNode is not (Comment or Space or Style)) prevDisplayedIndex = newList.Count; // Corresponds to atom types that use continue; in Typesetter.CreateLine
          newList.Add(newNode);
          prevNode = newNode;
        }
        if (prevDisplayedIndex >= 0
          && EffectiveClass(newList[prevDisplayedIndex]) == typeof(BinaryOperator))
          newList[prevDisplayedIndex] = DemoteBinary(newList[prevDisplayedIndex]);
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
    public override int GetHashCode() {
      // Keep hashing structural just like EqualsList. In particular, a cloned list
      // must remain hash-equal even though it has a different List<MathAtom> instance.
      unchecked {
        var hash = 17;
        foreach (var atom in Atoms) hash = hash * 31 + atom.GetHashCode();
        return Atoms.Count == 0 ? 0 : hash; // Preserve the LaTeXDefaults empty-list special case.
      }
    }
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
