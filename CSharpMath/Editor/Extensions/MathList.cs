using System;


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
      if (index is null || index.AtomIndex < 0 || index.AtomIndex >= self.Atoms.Count) return null;
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
        (MathListSubIndexType.TableRow, var rowIndex) => atom is Atoms.Table table
          && rowIndex.AtomIndex >= 0 && rowIndex.AtomIndex < table.Cells.Count
          && rowIndex.SubIndexInfo is (MathListSubIndexType.TableColumn, var columnIndex)
          && columnIndex.AtomIndex >= 0 && columnIndex.AtomIndex < table.Cells[rowIndex.AtomIndex].Count
          && columnIndex.SubIndexInfo is (MathListSubIndexType.TableCell, var cellIndex)
          ? table.Cells[rowIndex.AtomIndex][columnIndex.AtomIndex].AtomAt(cellIndex)
          : null,
        (var type, _) => throw new ArgumentOutOfRangeException(nameof(index), type, "Index type out of valid range."),
      };
    }
  }
}
