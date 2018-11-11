using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  using Interfaces;
  public static class MathListEditingExtensions {
    public static void Insert(this IMathList self, MathListIndex index, IMathAtom atom) {
      index = index ?? MathListIndex.Level0Index(0);
      if (index.AtomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexType) {
        case MathListSubIndexType.None:
          self.Insert(index.AtomIndex, atom);
          break;
        case MathListSubIndexType.Nucleus:
          var currentAtom = self.Atoms[index.AtomIndex];
          if (currentAtom.Subscript == null && currentAtom.Superscript == null)
            throw new SubIndexTypeMismatchException("Nuclear fusion is not supported if there are neither subscripts nor superscripts in the current atom.");
          if (atom.Subscript != null || atom.Superscript != null)
            throw new ArgumentException("Cannot fuse with an atom that already has a subscript or a superscript");
          atom.Subscript = currentAtom.Subscript;
          atom.Superscript = currentAtom.Superscript;
          currentAtom.Subscript = null;
          currentAtom.Superscript = null;
          self.Insert(index.AtomIndex + index.SubIndex?.AtomIndex ?? 0, atom);
          break;
        case MathListSubIndexType.Degree:
        case MathListSubIndexType.Radicand:
          if (!(self.Atoms[index.AtomIndex] is Radical radical && radical.AtomType == Enumerations.MathAtomType.Radical))
            throw new SubIndexTypeMismatchException($"No radical found at index {index.AtomIndex}");
          if (index.SubIndexType == MathListSubIndexType.Degree)
            radical.Degree.Insert(index.SubIndex, atom);
          else
            radical.Radicand.Insert(index.SubIndex, atom);
          break;
        case MathListSubIndexType.Numerator:
        case MathListSubIndexType.Denominator:
          if (!(self.Atoms[index.AtomIndex] is Fraction frac && frac.AtomType == Enumerations.MathAtomType.Fraction))
            throw new SubIndexTypeMismatchException($"No fraction found at index {index.AtomIndex}");
          if (index.SubIndexType == MathListSubIndexType.Numerator)
            frac.Numerator.Insert(index.SubIndex, atom);
          else
            frac.Denominator.Insert(index.SubIndex, atom);
          break;
        case MathListSubIndexType.Subscript:
          var current = self.Atoms[index.AtomIndex];
          if (current.Subscript == null) throw new SubIndexTypeMismatchException($"No subscript for atom at index {index.AtomIndex}");
          current.Subscript.Insert(index.SubIndex, atom);
          break;
        case MathListSubIndexType.Superscript:
          current = self.Atoms[index.AtomIndex];
          if (current.Superscript == null) throw new SubIndexTypeMismatchException($"No superscript for atom at index {index.AtomIndex}");
          current.Superscript.Insert(index.SubIndex, atom);
          break;
      }
    }

    public static void RemoveAt(this IMathList self, MathListIndex index) {
      index = index ?? MathListIndex.Level0Index(0);
      if (index.AtomIndex > self.Atoms.Count)
        throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
      switch (index.SubIndexType) {
        case MathListSubIndexType.None:
          self.RemoveAt(index.AtomIndex);
          break;
        case MathListSubIndexType.Nucleus:
          var currentAtom = self.Atoms[index.AtomIndex];
          if (currentAtom.Subscript == null && currentAtom.Superscript == null)
            throw new SubIndexTypeMismatchException("Nuclear fission is not supported if there are no subscripts or superscripts.");
          if (index.AtomIndex > 0) {
            var previous = self.Atoms[index.AtomIndex - 1];
            if (previous.Subscript != null && previous.Superscript != null) {
              previous.Superscript = currentAtom.Superscript;
              previous.Subscript = currentAtom.Subscript;
              self.RemoveAt(index.AtomIndex);
              break;
            }
          }
          // no previous atom or the previous atom sucks (has sub/super scripts)
          currentAtom.Nucleus = "";
          break;
        case MathListSubIndexType.Radicand:
        case MathListSubIndexType.Degree:
          if (!(self.Atoms[index.AtomIndex] is Radical radical && radical.AtomType == Enumerations.MathAtomType.Radical))
            throw new SubIndexTypeMismatchException($"No radical found at index {index.AtomIndex}");
          if (index.SubIndexType == MathListSubIndexType.Degree)
            radical.Degree.RemoveAt(index.SubIndex);
          else
            radical.Radicand.RemoveAt(index.SubIndex);
          break;
        case MathListSubIndexType.Numerator:
        case MathListSubIndexType.Denominator:
          if (!(self.Atoms[index.AtomIndex] is Fraction frac && frac.AtomType == Enumerations.MathAtomType.Fraction))
            throw new SubIndexTypeMismatchException($"No fraction found at index {index.AtomIndex}");
          if (index.SubIndexType == MathListSubIndexType.Numerator)
            frac.Numerator.RemoveAt(index.SubIndex);
          else
            frac.Denominator.RemoveAt(index.SubIndex);
          break;
        case MathListSubIndexType.Subscript:
          var current = self.Atoms[index.AtomIndex];
          if (current.Subscript == null) throw new SubIndexTypeMismatchException($"No subscript for atom at index {index.AtomIndex}");
          current.Subscript.RemoveAt(index.SubIndex);
          break;
        case MathListSubIndexType.Superscript:
          current = self.Atoms[index.AtomIndex];
          if (current.Superscript == null) throw new SubIndexTypeMismatchException($"No superscript for atom at index {index.AtomIndex}");
          current.Superscript.RemoveAt(index.SubIndex);
          break;
      }
    }

    public static void RemoveAtoms(this IMathList self, MathListRange? nullableRange) {
      if (!nullableRange.HasValue) return;
      var range = nullableRange.GetValueOrDefault();
      var start = range.Start;
      switch (start.SubIndexType) {
        case MathListSubIndexType.None:
          self.RemoveAtoms(new Range(start.AtomIndex, range.Length));
          break;
        case MathListSubIndexType.Nucleus:
          throw new NotSupportedException("Nuclear fission is not supported");
        case MathListSubIndexType.Radicand:
        case MathListSubIndexType.Degree:
          if (!(self.Atoms[start.AtomIndex] is Radical radical && radical.AtomType == Enumerations.MathAtomType.Radical))
            throw new SubIndexTypeMismatchException($"No radical found at index {start.AtomIndex}");
          if (start.SubIndexType == MathListSubIndexType.Degree)
            radical.Degree.RemoveAtoms(range.SubIndexRange);
          else
            radical.Radicand.RemoveAtoms(range.SubIndexRange);
          break;
        case MathListSubIndexType.Numerator:
        case MathListSubIndexType.Denominator:
          if (!(self.Atoms[start.AtomIndex] is Fraction frac && frac.AtomType == Enumerations.MathAtomType.Fraction))
            throw new SubIndexTypeMismatchException($"No fraction found at index {start.AtomIndex}");
          if (start.SubIndexType == MathListSubIndexType.Numerator)
            frac.Numerator.RemoveAtoms(range.SubIndexRange);
          else
            frac.Denominator.RemoveAtoms(range.SubIndexRange);
          break;
        case MathListSubIndexType.Subscript:
          var current = self.Atoms[start.AtomIndex];
          if (current.Subscript == null) throw new SubIndexTypeMismatchException($"No subscript for atom at index {start.AtomIndex}");
          current.Subscript.RemoveAtoms(range.SubIndexRange);
          break;
        case MathListSubIndexType.Superscript:
          current = self.Atoms[start.AtomIndex];
          if (current.Superscript == null) throw new SubIndexTypeMismatchException($"No superscript for atom at index {start.AtomIndex}");
          current.Superscript.RemoveAtoms(range.SubIndexRange);
          break;
      }
    }
    
    [NullableReference]
    public static IMathAtom AtomAt(this IMathList self, MathListIndex index) {
      if (index is null || index.AtomIndex >= self.Atoms.Count) return null;
      var atom = self.Atoms[index.AtomIndex];
      switch (index.SubIndexType) {
        case MathListSubIndexType.None:
        case MathListSubIndexType.Nucleus:
          return atom;
        case MathListSubIndexType.Subscript:
          return atom.Subscript.AtomAt(index.SubIndex);
        case MathListSubIndexType.Superscript:
          return atom.Superscript.AtomAt(index.SubIndex);
        case MathListSubIndexType.Radicand:
        case MathListSubIndexType.Degree:
          if (atom is Radical radical && radical.AtomType == Enumerations.MathAtomType.Radical)
            if (index.SubIndexType == MathListSubIndexType.Degree)
              return radical.Degree.AtomAt(index.SubIndex);
            else
              return radical.Radicand.AtomAt(index.SubIndex);
          else
            return null;
        case MathListSubIndexType.Numerator:
        case MathListSubIndexType.Denominator:
          if (atom is Fraction frac && frac.AtomType == Enumerations.MathAtomType.Fraction)
            if (index.SubIndexType == MathListSubIndexType.Denominator)
              return frac.Denominator.AtomAt(index.SubIndex);
            else
              return frac.Numerator.AtomAt(index.SubIndex);
          else
            return null;
        default:
          return null;
      }
    }
  }
}
