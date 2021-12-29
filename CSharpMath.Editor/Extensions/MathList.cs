using System;

namespace CSharpMath.Editor {
  using Atom;
  using Atoms = Atom.Atoms;
  using Structures;
  using System.Diagnostics;
  partial class Extensions {
    /// <summary>Inserts <paramref name="atom"/> at position <paramref name="atomIndex"/> inside the leaf self,
    /// returning a resultant index relative to self.</summary>
    static MathListIndex InsertAtAtomIndexAndAdvance(this MathList self, int atomIndex, MathAtom atom, MathListSubIndexType? advanceType) {
      if (atomIndex < 0 || atomIndex > self.Count)
        throw new IndexOutOfRangeException($"Index {atomIndex} is out of bounds for list of size {self.Atoms.Count}");
      // Test for placeholder to the right of index, e.g. \sqrt{‸■} -> \sqrt{2‸}
      if (atomIndex < self.Count && self[atomIndex] is Atoms.Placeholder placeholder) {
        atom.Superscript.Append(placeholder.Superscript);
        atom.Subscript.Append(placeholder.Subscript);
        self[atomIndex] = atom;
      } else self.Insert(atomIndex, atom);
      return advanceType switch
      {
        null => MathListIndex.Level0Index(atomIndex + 1),
        MathListSubIndexType advanceT => new MathListIndex(atomIndex, (advanceT, MathListIndex.Level0Index(0))),
      };
    }
    /// <summary>Inserts <paramref name="atom"/> at <paramref name="index"/> relative to self,
    /// returning an updated relative index.</summary>
    public static MathListIndex InsertAndAdvance(this MathList self, MathListIndex index, MathAtom atom, MathListSubIndexType? advanceType) {
      int atomIndex = index.AtomIndex;
      if (atomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexInfo) {
        case null:
          return self.InsertAtAtomIndexAndAdvance(atomIndex, atom, advanceType);
        case (MathListSubIndexType type, MathListIndex subIndex):
          switch (type) {
            case MathListSubIndexType.BetweenBaseAndScripts: // TODO: finish this section
              {
                var currentAtom = self.Atoms[index.AtomIndex];
                if (currentAtom.Subscript.IsEmpty() && currentAtom.Superscript.IsEmpty())
                  throw new SubIndexTypeMismatchException(index);
                if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty())
                  throw new ArgumentException("Cannot fuse with an atom that already has a subscript or a superscript");
                atom.Subscript.Append(currentAtom.Subscript);
                atom.Superscript.Append(currentAtom.Superscript);
                currentAtom.Subscript.Clear();
                currentAtom.Superscript.Clear();
                // Prevent further subindexing inside BetweenBaseAndScripts
                if (advanceType != null
                    && index.LevelDown() is MathListIndex levelDown) index = levelDown.Next;
                return self.InsertAtAtomIndexAndAdvance(atomIndex + 1, atom, advanceType);
              }
            case MathListSubIndexType.Degree:
              {
                if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
                return new MathListIndex(atomIndex,
                  (MathListSubIndexType.Degree, radical.Degree.InsertAndAdvance(subIndex, atom, advanceType)));
              }
            case MathListSubIndexType.Radicand:
              {
                if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
                return new MathListIndex(atomIndex,
                  (MathListSubIndexType.Radicand, radical.Radicand.InsertAndAdvance(subIndex, atom, advanceType)));
              }
            case MathListSubIndexType.Numerator:
              {
                if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
                return new MathListIndex(atomIndex,
                  (MathListSubIndexType.Radicand, frac.Numerator.InsertAndAdvance(subIndex, atom, advanceType)));
              }
            case MathListSubIndexType.Denominator:
              {
                if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
                  throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
                return new MathListIndex(atomIndex,
                  (MathListSubIndexType.Radicand, frac.Denominator.InsertAndAdvance(subIndex, atom, advanceType)));
              }
            case MathListSubIndexType.Subscript:
              return new MathListIndex(atomIndex,
                ( MathListSubIndexType.Subscript,
                  self.Atoms[index.AtomIndex].Subscript.InsertAndAdvance(subIndex, atom, advanceType)));
            case MathListSubIndexType.Superscript:
              return new MathListIndex(atomIndex,
                ( MathListSubIndexType.Superscript,
                  self.Atoms[index.AtomIndex].Superscript.InsertAndAdvance(subIndex, atom, advanceType)));
            case MathListSubIndexType.Inner:
              if (!(self.Atoms[index.AtomIndex] is Atoms.Inner inner))
                throw new SubIndexTypeMismatchException(typeof(Atoms.Inner), index);
              return new MathListIndex(atomIndex,
                ( MathListSubIndexType.Inner,
                  inner.InnerList.InsertAndAdvance(subIndex, atom, advanceType)));
            default:
              throw new SubIndexTypeMismatchException(index);
          }
        default: throw new Exception("Can switch expressions be nested?");
      }
    }

    public static void RemoveAt(this MathList self, MathListIndex index) {
      if (index.AtomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexInfo) {
        case null:
          self.RemoveAt(index.AtomIndex);
          break;
        case (MathListSubIndexType.BetweenBaseAndScripts,_): { // TODO fix this
          var currentAtom = self.Atoms[index.AtomIndex];
          if (currentAtom.Subscript.IsEmpty() && currentAtom.Superscript.IsEmpty())
            throw new SubIndexTypeMismatchException(index);
          var downIndex = index.LevelDown();
          if (downIndex is null) throw new InvalidCodePathException("downIndex is null");
          if (index.AtomIndex > 0 &&
              self.Atoms[index.AtomIndex - 1] is MathAtom previous &&
              previous.Subscript.IsEmpty() &&
              previous.Superscript.IsEmpty() &&
              previous switch
              {
                Atoms.BinaryOperator _ => false,
                Atoms.UnaryOperator _ => false,
                Atoms.Relation _ => false,
                Atoms.Punctuation _ => false,
                Atoms.Space _ => false,
                _ => true
              })
          {
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
          self.InsertAndAdvance(index, insertionAtom, null);
          index = index.Previous ?? throw new InvalidCodePathException("Cannot go back after insertion?");
          return; }
        case (MathListSubIndexType.Radicand, MathListIndex subIndex):
          {
            if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
            radical.Radicand.RemoveAt(subIndex);
            break;
          }
        case (MathListSubIndexType.Degree, MathListIndex subIndex):
          {
            if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
            radical.Degree.RemoveAt(subIndex);
            break;
          }
        case (MathListSubIndexType.Numerator, MathListIndex subIndex):
          {
            if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
            frac.Numerator.RemoveAt(subIndex);
            break;
          }
        case (MathListSubIndexType.Denominator, MathListIndex subIndex):
          {
            if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
            frac.Denominator.RemoveAt(subIndex);
            break;
          }
        case (MathListSubIndexType.Subscript, MathListIndex subIndex):
          var current = self.Atoms[index.AtomIndex];
          if (current.Subscript.IsEmpty())
            throw new SubIndexTypeMismatchException(index);
          current.Subscript.RemoveAt(subIndex);
          break;
        case (MathListSubIndexType.Superscript, MathListIndex subIndex):
          current = self.Atoms[index.AtomIndex];
          if (current.Superscript.IsEmpty())
            throw new SubIndexTypeMismatchException(index);
          current.Superscript.RemoveAt(subIndex);
          break;
        case (MathListSubIndexType.Inner, MathListIndex subIndex):
          if (!(self.Atoms[index.AtomIndex] is Atoms.Inner inner))
            throw new SubIndexTypeMismatchException(typeof(Atoms.Inner), index);
          inner.InnerList.RemoveAt(subIndex);
          break;
        default:
          throw new SubIndexTypeMismatchException(index);
      }
      if (index.Previous is null && index.SubIndexInfo != null) {
        // We have deleted to the beginning of the line and it is not the outermost line
        if (self.AtomAt(index) is null) {
          self.InsertAndAdvance(index, LaTeXSettings.Placeholder, null);
          //index = index.Previous ?? throw new InvalidCodePathException("Cannot go back after insertion?"); ;
        }
      }
    }

    public static void RemoveAtoms(this MathList self, MathListRange? nullableRange) {
      if (!(nullableRange is MathListRange range)) return;
      var start = range.Start;
      switch (start.SubIndexInfo) {
        case null:
          self.RemoveAtoms(start.AtomIndex, range.Length);
          break;
        case (MathListSubIndexType.BetweenBaseAndScripts,_):
          throw new NotSupportedException("Nuclear fission is not supported");
        case (MathListSubIndexType.Radicand, _):
          {
            if (!(self.Atoms[start.AtomIndex] is Atoms.Radical radical))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), start);
            radical.Radicand.RemoveAtoms(range.SubIndexRange);
            break;
          }
        case (MathListSubIndexType.Degree, _):
          {
            if (!(self.Atoms[start.AtomIndex] is Atoms.Radical radical))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), start);
            radical.Degree.RemoveAtoms(range.SubIndexRange);
            break;
          }
        case (MathListSubIndexType.Numerator,_):
          {
            if (!(self.Atoms[start.AtomIndex] is Atoms.Fraction frac))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), start);
            frac.Numerator.RemoveAtoms(range.SubIndexRange);
            break;
          }
        case (MathListSubIndexType.Denominator,_):
          {
            if (!(self.Atoms[start.AtomIndex] is Atoms.Fraction frac))
              throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), start);
            frac.Denominator.RemoveAtoms(range.SubIndexRange);
            break;
          }
        case (MathListSubIndexType.Subscript,_):
          var current = self.Atoms[start.AtomIndex];
          if (current.Subscript.IsEmpty()) throw new SubIndexTypeMismatchException(start);
          current.Subscript.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Superscript,_):
          current = self.Atoms[start.AtomIndex];
          if (current.Superscript.IsEmpty()) throw new SubIndexTypeMismatchException(start);
          current.Superscript.RemoveAtoms(range.SubIndexRange);
          break;
        case (MathListSubIndexType.Inner,_):
          if (!(self.Atoms[start.AtomIndex] is Atoms.Inner inner))
            throw new SubIndexTypeMismatchException(typeof(Atoms.Inner), start);
          inner.InnerList.RemoveAtoms(range.SubIndexRange);
          break;
      }
    }

    public static MathAtom? AtomAt(this MathList self, MathListIndex? index) {
      if (index is null || index.AtomIndex >= self.Atoms.Count) return null;
      var atom = self.Atoms[index.AtomIndex];
      switch (index.SubIndexInfo) {
        case null:
          return atom;
        case (MathListSubIndexType.BetweenBaseAndScripts,_):
          return null;
        case (MathListSubIndexType.Subscript, MathListIndex subIndex):
          return atom.Subscript.AtomAt(subIndex);
        case (MathListSubIndexType.Superscript, MathListIndex subIndex):
          return atom.Superscript.AtomAt(subIndex);
        case (MathListSubIndexType.Radicand, MathListIndex subIndex):
          {
            return
              atom is Atoms.Radical radical
              ? radical.Radicand.AtomAt(subIndex)
              : null;
          }
        case (MathListSubIndexType.Degree, MathListIndex subIndex):
          {
            return
              atom is Atoms.Radical radical
              ? radical.Degree.AtomAt(subIndex)
              : null;
          }
        case (MathListSubIndexType.Numerator, MathListIndex subIndex):
          {
            return
              atom is Atoms.Fraction frac
              ? frac.Numerator.AtomAt(subIndex)
              : null;
          }
        case (MathListSubIndexType.Denominator, MathListIndex subIndex):
          {
            return
              atom is Atoms.Fraction frac
              ? frac.Denominator.AtomAt(subIndex)
              : null;
          }
        case (MathListSubIndexType.Inner, MathListIndex subIndex):
          return atom is Atoms.Inner inner ? inner.InnerList.AtomAt(subIndex) : null;
        default:
          throw new SubIndexTypeMismatchException(index);
      }
    }
  }
}
