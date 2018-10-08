using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  public static class MathListEditingExtensions {
    public static void Insert(this Interfaces.IMathList self, MathListIndex index, MathAtom atom) {
      index = index ?? new MathListIndex { AtomIndex = 0, SubIndex = null, SubIndexType = MathListSubIndexType.None };
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

    public static void RemoveAt(this Interfaces.IMathList self, MathListIndex index) {
      index = index ?? new MathListIndex { AtomIndex = 0, SubIndex = null, SubIndexType = MathListSubIndexType.None };
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

    public static void RemoveAtoms(Interfaces.IMathList self, Range range) {
      var start = range.Location;
      new MathListIndex {  }
    }
  }
}
