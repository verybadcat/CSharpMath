using System;
using System.Collections.Generic;


namespace CSharpMath.Editor {
  using Atom;
  using Atoms = Atom.Atoms;
  partial class Extensions {
    static MathListIndex InsertAtAtomIndexAndAdvance(this MathList self, int atomIndex, MathAtom atom, MathListIndex advance, MathListSubIndexType? advanceType) {
      if (atomIndex < 0 || atomIndex > self.Count)
        throw new IndexOutOfRangeException($"Index {atomIndex} is out of bounds for list of size {self.Atoms.Count}");
      // Test for placeholder to the right of index, e.g. \sqrt{‸■} -> \sqrt{2‸}
      if (atomIndex < self.Count && self[atomIndex] is Atoms.Placeholder placeholder) {
        atom.Superscript.Append(placeholder.Superscript);
        atom.Subscript.Append(placeholder.Subscript);
        self[atomIndex] = atom;
      } else self.Insert(atomIndex, atom);
      return advanceType switch {
        { } a => advance.LevelUpWithSubIndex(a, 0),
        null => advance.Next,
      };
    }
    /// <summary>Inserts <paramref name="atom"/> and returns a new <see cref="MathListIndex"/> with <paramref name="index"/> advanced to the next position which is another subindex level if <paramref name="advanceType"/> is not <see langword="null"/>.</summary>
    public static MathListIndex InsertAndAdvance(this MathList self, MathListIndex index, MathAtom atom, MathListSubIndexType? advanceType) {
      if (index.AtomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexInfo) {
        case null:
          return self.InsertAtAtomIndexAndAdvance(index.AtomIndex, atom, index, advanceType);
        case (MathListSubIndexType.BetweenBaseAndScripts, _):
          var currentAtom = self.Atoms[index.AtomIndex];
          if (currentAtom.Subscript.IsEmpty() && currentAtom.Superscript.IsEmpty())
            throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.BetweenBaseAndScripts), index.AtomIndex);
          if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty())
            throw new ArgumentException("Cannot fuse with an atom that already has a subscript or a superscript");
          atom.Subscript.Append(currentAtom.Subscript);
          atom.Superscript.Append(currentAtom.Superscript);
          currentAtom.Subscript.Clear();
          currentAtom.Superscript.Clear();
          var atomIndex = index.AtomIndex;
          // Prevent further subindexing inside BetweenBaseAndScripts
          if (advanceType is { } && index.LevelDown() is MathListIndex levelDown) index = levelDown.Next;
          return self.InsertAtAtomIndexAndAdvance(atomIndex + 1, atom, index, advanceType);
        case (MathListSubIndexType.Degree, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), index.AtomIndex):
          return radical.Degree.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Degree);
        case (MathListSubIndexType.Radicand, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), index.AtomIndex):
          return radical.Radicand.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Radicand);
        case (MathListSubIndexType.Numerator, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), index.AtomIndex):
          return frac.Numerator.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Numerator);
        case (MathListSubIndexType.Denominator, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), index.AtomIndex):
          return frac.Denominator.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Denominator);
        case (MathListSubIndexType.Subscript, var subIndex):
          return self.Atoms[index.AtomIndex].Subscript.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Subscript);
        case (MathListSubIndexType.Superscript, var subIndex):
          return self.Atoms[index.AtomIndex].Superscript.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Superscript);
        case (MathListSubIndexType.Inner, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Inner inner ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Inner), index.AtomIndex):
          return inner.InnerList.InsertAndAdvance(subIndex, atom, advanceType).Wrap(index.AtomIndex, MathListSubIndexType.Inner);
        case (var type, _):
          throw new ArgumentOutOfRangeException(nameof(index), type, "Index type out of valid range.");
      }
    }
    /// <summary>Removes the atom at <paramref name="index"/>, placing a new placeholder if all atoms are removed, and returns a new <see cref="MathListIndex"/> with <paramref name="index"/> advanced to the next appropriate position.</summary>
    public static MathListIndex RemoveAt(this MathList self, MathListIndex index) {
      if (index.AtomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexInfo) {
        case null:
          self.RemoveAt(index.AtomIndex);
          break;
        case (MathListSubIndexType.BetweenBaseAndScripts, _):
          var currentAtom = self.Atoms[index.AtomIndex];
          if (currentAtom.Subscript.IsEmpty() && currentAtom.Superscript.IsEmpty())
            throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.BetweenBaseAndScripts), index.AtomIndex);
          var downIndex = index.LevelDown() ?? throw new InvalidCodePathException("downIndex is null");
          if (index.AtomIndex > 0 &&
              self.Atoms[index.AtomIndex - 1] is MathAtom previous &&
              previous.Subscript.IsEmpty() &&
              previous.Superscript.IsEmpty() &&
              previous switch {
                Atoms.BinaryOperator _ => false,
                Atoms.UnaryOperator _ => false,
                Atoms.Relation _ => false,
                Atoms.Punctuation _ => false,
                Atoms.Space _ => false,
                _ => true
              }) {
            previous.Superscript.Append(currentAtom.Superscript);
            previous.Subscript.Append(currentAtom.Subscript);
            self.RemoveAt(index.AtomIndex);
            // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
            index = downIndex.Previous is MathListIndex downPrev
              ? downPrev.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, 1)
              : downIndex;
            break;
          }
          // insert placeholder since we couldn't place the scripts in previous atom
          var insertionAtom = LaTeXSettings.Placeholder;
          insertionAtom.Subscript.Append(currentAtom.Subscript);
          insertionAtom.Superscript.Append(currentAtom.Superscript);
          self.RemoveAt(index.AtomIndex);
          index = downIndex;
          index = self.InsertAndAdvance(index, insertionAtom, null);
          return index.Previous ?? throw new InvalidCodePathException("Cannot go back after insertion?");
        case (MathListSubIndexType.Degree, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), index.AtomIndex):
          index = radical.Degree.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Degree);
          break;
        case (MathListSubIndexType.Radicand, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), index.AtomIndex):
          index = radical.Radicand.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Radicand);
          break;
        case (MathListSubIndexType.Numerator, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), index.AtomIndex):
          index = frac.Numerator.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Numerator);
          break;
        case (MathListSubIndexType.Denominator, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), index.AtomIndex):
          index = frac.Denominator.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Denominator);
          break;
        case (MathListSubIndexType.Subscript, var subIndex):
          var current = self.Atoms[index.AtomIndex];
          if (current.Subscript.IsEmpty())
            throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.Subscript), index.AtomIndex);
          index = current.Subscript.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Subscript);
          break;
        case (MathListSubIndexType.Superscript, var subIndex):
          current = self.Atoms[index.AtomIndex];
          if (current.Superscript.IsEmpty())
            throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.Superscript), index.AtomIndex);
          index = current.Superscript.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Superscript);
          break;
        case (MathListSubIndexType.Inner, var subIndex)
          when self.Atoms[index.AtomIndex] is Atoms.Inner inner ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Inner), index.AtomIndex):
          index = inner.InnerList.RemoveAt(subIndex).Wrap(index.AtomIndex, MathListSubIndexType.Inner);
          break;
        case (var type, _):
          throw new ArgumentOutOfRangeException(nameof(index), type, "Index type out of valid range.");
      }
      if (index.Previous is null && index.SubIndexInfo is { })
        // We have deleted to the beginning of the line and it is not the outermost line
        if (self.AtomAt(index) is null)
          self.InsertAndAdvance(index, LaTeXSettings.Placeholder, null);
      return index;
    }

    static bool IsPlaceholderOnly(MathList list) =>
      list.Count == 1 && list[0] is Atoms.Placeholder {
        Subscript.Count: 0,
        Superscript.Count: 0
      };

    static MathList ChildList(MathAtom atom, MathListSubIndexType type) => type switch {
      MathListSubIndexType.Numerator => ((Atoms.Fraction)atom).Numerator,
      MathListSubIndexType.Denominator => ((Atoms.Fraction)atom).Denominator,
      MathListSubIndexType.Degree => ((Atoms.Radical)atom).Degree,
      MathListSubIndexType.Radicand => ((Atoms.Radical)atom).Radicand,
      MathListSubIndexType.Inner => ((Atoms.Inner)atom).InnerList,
      MathListSubIndexType.Subscript => atom.Subscript,
      MathListSubIndexType.Superscript => atom.Superscript,
      _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    static List<MathAtom> ContentOf(MathList list) =>
      IsPlaceholderOnly(list) ? new List<MathAtom>() : new List<MathAtom>(list.Atoms);

    static void InsertAll(MathList list, int index, List<MathAtom> atoms) {
      for (var i = 0; i < atoms.Count; i++) list.Insert(index + i, atoms[i]);
    }

    /// <summary>
    /// Removes a placeholder at the caret and moves its script contents into the containing list.
    /// The returned caret stays before the moved content.
    /// </summary>
    internal static MathListIndex RemovePlaceholderAtCaret(this MathList self, MathListIndex index) {
      if (index.AtomIndex < 0 || index.AtomIndex >= self.Count)
        throw new IndexOutOfRangeException(nameof(index));
      if (index.SubIndexInfo is var (type, childIndex)) {
        var atom = self[index.AtomIndex];
        var child = ChildList(atom, type);
        var childResult = child.RemovePlaceholderAtCaret(childIndex);
        if (child.IsEmpty()) {
          child.Add(LaTeXSettings.Placeholder);
          childResult = new(0);
        }
        return childResult.Wrap(index.AtomIndex, type);
      }
      if (self[index.AtomIndex] is not Atoms.Placeholder placeholder)
        throw new InvalidOperationException("Expected a placeholder at the caret");
      var replacement = ContentOf(placeholder.Subscript);
      replacement.AddRange(ContentOf(placeholder.Superscript));
      self.RemoveAt(index.AtomIndex);
      InsertAll(self, index.AtomIndex, replacement);
      return new(index.AtomIndex);
    }

    /// <summary>
    /// Simplifies the innermost structure whose field begins at <paramref name="index"/>.
    /// Content before that field remains before the returned caret; the field and later
    /// sibling content remain after it.
    /// </summary>
    internal static MathListIndex SimplifyContainingAtBeginning(this MathList self, MathListIndex index) {
      if (index.SubIndexInfo is not var (type, childIndex))
        throw new ArgumentException("The caret is not inside a structured field", nameof(index));
      if (index.AtomIndex < 0 || index.AtomIndex >= self.Count)
        throw new IndexOutOfRangeException(nameof(index));
      var atom = self[index.AtomIndex];
      var child = ChildList(atom, type);
      if (childIndex.SubIndexInfo is not null) {
        var childResult = child.SimplifyContainingAtBeginning(childIndex);
        if (child.IsEmpty()) {
          child.Add(LaTeXSettings.Placeholder);
          childResult = new(0);
        }
        return childResult.Wrap(index.AtomIndex, type);
      }
      if (childIndex.AtomIndex != 0)
        throw new ArgumentException("The caret is not at the beginning of its field", nameof(index));

      if (type is MathListSubIndexType.Subscript or MathListSubIndexType.Superscript) {
        var moved = ContentOf(child);
        child.Clear();
        if (atom is Atoms.LargeOperator) {
          var subscript = type == MathListSubIndexType.Subscript ? moved : ContentOf(atom.Subscript);
          var superscript = type == MathListSubIndexType.Superscript ? moved : ContentOf(atom.Superscript);
          var replacement = new List<MathAtom>(subscript);
          var caretOffset = type == MathListSubIndexType.Subscript ? 0 : replacement.Count;
          replacement.AddRange(superscript);
          self.RemoveAt(index.AtomIndex);
          InsertAll(self, index.AtomIndex, replacement);
          return new(index.AtomIndex + caretOffset);
        }
        var other = type == MathListSubIndexType.Subscript ? atom.Superscript : atom.Subscript;
        if (atom is Atoms.Placeholder && other.IsEmpty()) {
          self.RemoveAt(index.AtomIndex);
          InsertAll(self, index.AtomIndex, moved);
          return new(index.AtomIndex);
        }
        InsertAll(self, index.AtomIndex + 1, moved);
        return new(index.AtomIndex + 1);
      }

      if (atom is not IMathListContainer container)
        throw new SubIndexTypeMismatchException(type.ToString(), index.AtomIndex);
      var replacementAtoms = new List<MathAtom>();
      var caretOffsetInReplacement = -1;
      foreach (var innerList in container.InnerLists) {
        if (ReferenceEquals(innerList, child)) caretOffsetInReplacement = replacementAtoms.Count;
        replacementAtoms.AddRange(ContentOf(innerList));
      }
      if (caretOffsetInReplacement < 0)
        throw new InvalidOperationException("The selected field is not owned by its container");
      if (replacementAtoms.Count > 0) {
        replacementAtoms[replacementAtoms.Count - 1].Subscript.Append(atom.Subscript);
        replacementAtoms[replacementAtoms.Count - 1].Superscript.Append(atom.Superscript);
      } else if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
        var placeholder = LaTeXSettings.Placeholder;
        placeholder.Subscript.Append(atom.Subscript);
        placeholder.Superscript.Append(atom.Superscript);
        replacementAtoms.Add(placeholder);
      }
      self.RemoveAt(index.AtomIndex);
      InsertAll(self, index.AtomIndex, replacementAtoms);
      return new(index.AtomIndex + caretOffsetInReplacement);
    }

    public static void RemoveAtoms(this MathList self, MathListRange? nullableRange) {
      if (nullableRange is not MathListRange range) return;
      var start = range.Start;
      switch (start.SubIndexInfo) {
        case null:
          self.RemoveAtoms(start.AtomIndex, range.Length);
          break;
        case (MathListSubIndexType.BetweenBaseAndScripts, _):
          throw new NotSupportedException("Nuclear fission is not supported");
        case (MathListSubIndexType.Degree, _)
          when self.Atoms[start.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), start.AtomIndex):
          radical.Degree.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Radicand, _)
          when self.Atoms[start.AtomIndex] is Atoms.Radical radical ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Radical), start.AtomIndex):
          radical.Radicand.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Numerator, _)
          when self.Atoms[start.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), start.AtomIndex):
          frac.Numerator.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Denominator, _)
          when self.Atoms[start.AtomIndex] is Atoms.Fraction frac ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Fraction), start.AtomIndex):
          frac.Denominator.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Subscript, _):
          var current = self.Atoms[start.AtomIndex];
          if (current.Subscript.IsEmpty()) throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.Subscript), start.AtomIndex);
          current.Subscript.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Superscript, _):
          current = self.Atoms[start.AtomIndex];
          if (current.Superscript.IsEmpty()) throw new SubIndexTypeMismatchException(nameof(MathListSubIndexType.Superscript), start.AtomIndex);
          current.Superscript.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Inner, _)
          when self.Atoms[start.AtomIndex] is Atoms.Inner inner ? true
               : throw new SubIndexTypeMismatchException(nameof(Atoms.Inner), start.AtomIndex):
          inner.InnerList.RemoveAtoms(range.SubIndexRange);
          break;
      }
    }

    public static MathAtom? AtomAt(this MathList self, MathListIndex? index) {
      if (index is null || index.AtomIndex >= self.Atoms.Count) return null;
      var atom = self.Atoms[index.AtomIndex];
      return index.SubIndexInfo switch {
        null => atom,
        (MathListSubIndexType.BetweenBaseAndScripts, _) => null,
        (MathListSubIndexType.Subscript, var subIndex) => atom.Subscript.AtomAt(subIndex),
        (MathListSubIndexType.Superscript, var subIndex) => atom.Superscript.AtomAt(subIndex),
        (MathListSubIndexType.Degree, var subIndex) => atom is Atoms.Radical radical ? radical.Degree.AtomAt(subIndex) : null,
        (MathListSubIndexType.Radicand, var subIndex) => atom is Atoms.Radical radical ? radical.Radicand.AtomAt(subIndex) : null,
        (MathListSubIndexType.Numerator, var subIndex) => atom is Atoms.Fraction frac ? frac.Numerator.AtomAt(subIndex) : null,
        (MathListSubIndexType.Denominator, var subIndex) => atom is Atoms.Fraction frac ? frac.Denominator.AtomAt(subIndex) : null,
        (MathListSubIndexType.Inner, var subIndex) => atom is Atoms.Inner inner ? inner.InnerList.AtomAt(subIndex) : null,
        (var type, _) => throw new ArgumentOutOfRangeException(nameof(index), type, "Index type out of valid range."),
      };
    }
  }
}
