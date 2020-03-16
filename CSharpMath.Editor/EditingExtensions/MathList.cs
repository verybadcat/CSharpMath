using System;

namespace CSharpMath.Editor
{
    using Atom;
    using Atoms = Atom.Atoms;
    using Structures;
    partial class EditingExtensions
    {
        static void InsertAtAtomIndexAndAdvance(this MathList self, int atomIndex, MathAtom atom, ref MathListIndex advance, MathListSubIndexType advanceType)
        {
            if (atomIndex < 0 || atomIndex > self.Count)
                throw new IndexOutOfRangeException($"Index {atomIndex} is out of bounds for list of size {self.Atoms.Count}");
            // Test for placeholder to the right of index, e.g. \sqrt{‸■} -> \sqrt{2‸}
            if (atomIndex < self.Count && self[atomIndex] is Atoms.Placeholder placeholder)
            {
                if (placeholder.Superscript is MathList super)
                {
                    if (atom.Superscript != null) super.Append(atom.Superscript);
                    atom.Superscript = super;
                }
                if (placeholder.Subscript is MathList sub)
                {
                    if (atom.Subscript != null) sub.Append(atom.Subscript);
                    atom.Subscript = sub;
                }
                self[atomIndex] = atom;
            }
            else self.Insert(atomIndex, atom);
            advance = advanceType switch
            {
                MathListSubIndexType.None => advance.Next,
                _ => advance.LevelUpWithSubIndex(advanceType, MathListIndex.Level0Index(0)),
            };
        }
        /// <summary>Inserts <paramref name="atom"/> and modifies <paramref name="index"/> to advance to the next position.</summary>
        public static void InsertAndAdvance(this MathList self, ref MathListIndex index, MathAtom atom, MathListSubIndexType advanceType)
        {
            index ??= MathListIndex.Level0Index(0);
            if (index.AtomIndex > self.Atoms.Count)
                throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
            switch (index.SubIndexType)
            {
                case MathListSubIndexType.None:
                    self.InsertAtAtomIndexAndAdvance(index.AtomIndex, atom, ref index, advanceType);
                    break;
                case var _ when index.SubIndex is null:
                    throw new InvalidCodePathException("index.SubIndex is null despite non-None subindex type");
                case MathListSubIndexType.BetweenBaseAndScripts:
                    var currentAtom = self.Atoms[index.AtomIndex];
                    if (currentAtom.Subscript == null && currentAtom.Superscript == null)
                        throw new SubIndexTypeMismatchException(index);
                    if (atom.Subscript != null || atom.Superscript != null)
                        throw new ArgumentException("Cannot fuse with an atom that already has a subscript or a superscript");
                    atom.Subscript = currentAtom.Subscript;
                    atom.Superscript = currentAtom.Superscript;
                    currentAtom.Subscript = null;
                    currentAtom.Superscript = null;
                    var atomIndex = index.AtomIndex;
                    // Prevent further subindexing inside BetweenBaseAndScripts
                    if (advanceType != MathListSubIndexType.None
                        && index.LevelDown() is MathListIndex levelDown) index = levelDown.Next;
                    self.InsertAtAtomIndexAndAdvance(atomIndex + 1, atom, ref index, advanceType);
                    break;
                case MathListSubIndexType.Degree:
                case MathListSubIndexType.Radicand:
                    if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
                    if (index.SubIndexType == MathListSubIndexType.Degree)
                        if (radical.Degree is null) throw new SubIndexTypeMismatchException(index);
                        else radical.Degree.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    else radical.Radicand.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    break;
                case MathListSubIndexType.Numerator:
                case MathListSubIndexType.Denominator:
                    if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
                    if (index.SubIndexType == MathListSubIndexType.Numerator)
                        if (frac.Numerator is null)
                            throw new SubIndexTypeMismatchException(index);
                        else frac.Numerator.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    else if (frac.Denominator is null)
                        throw new SubIndexTypeMismatchException(index);
                    else
                        frac.Denominator.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    break;
                case MathListSubIndexType.Subscript:
                    var current = self.Atoms[index.AtomIndex];
                    if (current.Subscript == null) throw new SubIndexTypeMismatchException(index);
                    current.Subscript.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    break;
                case MathListSubIndexType.Superscript:
                    current = self.Atoms[index.AtomIndex];
                    if (current.Superscript == null) throw new SubIndexTypeMismatchException(index);
                    current.Superscript.InsertAndAdvance(ref index.SubIndex, atom, advanceType);
                    break;
                default:
                    throw new SubIndexTypeMismatchException(index);
            }
        }

        public static void RemoveAt(this MathList self, ref MathListIndex index)
        {
            index ??= MathListIndex.Level0Index(0);
            if (index.AtomIndex > self.Atoms.Count)
                throw new IndexOutOfRangeException($"Index {index.AtomIndex} is out of bounds for list of size {self.Atoms.Count}");
            switch (index.SubIndexType)
            {
                case MathListSubIndexType.None:
                    self.RemoveAt(index.AtomIndex);
                    break;
                case var _ when index.SubIndex is null:
                    throw new InvalidCodePathException("index.SubIndex is null despite non-None subindex type");
                case MathListSubIndexType.BetweenBaseAndScripts:
                    var currentAtom = self.Atoms[index.AtomIndex];
                    if (currentAtom.Subscript == null && currentAtom.Superscript == null)
                        throw new SubIndexTypeMismatchException(index);
                    var downIndex = index.LevelDown();
                    if (downIndex is null) throw new InvalidCodePathException("downIndex is null");
                    if (index.AtomIndex > 0 &&
                        self.Atoms[index.AtomIndex - 1] is MathAtom previous &&
                        previous.Subscript is null &&
                        previous.Superscript is null &&
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
                        previous.Superscript = currentAtom.Superscript;
                        previous.Subscript = currentAtom.Subscript;
                        self.RemoveAt(index.AtomIndex);
                        // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
                        index = downIndex.Previous is MathListIndex downPrev
                          ? downPrev.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1))
                          : downIndex;
                        break;
                    }
                    // insert placeholder since we couldn't place the scripts in previous atom
                    var insertionAtom = MathAtoms.Placeholder;
                    insertionAtom.Subscript = currentAtom.Subscript;
                    insertionAtom.Superscript = currentAtom.Superscript;
                    self.RemoveAt(index.AtomIndex);
                    index = downIndex;
                    self.InsertAndAdvance(ref index, insertionAtom, MathListSubIndexType.None);
                    index = index.Previous ?? throw new InvalidCodePathException("Cannot go back after insertion?");
                    return;
                case MathListSubIndexType.Radicand:
                case MathListSubIndexType.Degree:
                    if (!(self.Atoms[index.AtomIndex] is Atoms.Radical radical))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), index);
                    if (index.SubIndexType == MathListSubIndexType.Degree)
                        if (radical.Degree is null) throw new SubIndexTypeMismatchException(index);
                        else radical.Degree.RemoveAt(ref index.SubIndex);
                    else radical.Radicand.RemoveAt(ref index.SubIndex);
                    break;
                case MathListSubIndexType.Numerator:
                case MathListSubIndexType.Denominator:
                    if (!(self.Atoms[index.AtomIndex] is Atoms.Fraction frac))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), index);
                    if (index.SubIndexType == MathListSubIndexType.Numerator)
                        if (frac.Numerator is null) throw new SubIndexTypeMismatchException(index);
                        else frac.Numerator.RemoveAt(ref index.SubIndex);
                    else if (frac.Denominator is null) throw new SubIndexTypeMismatchException(index);
                    else frac.Denominator.RemoveAt(ref index.SubIndex);
                    break;
                case MathListSubIndexType.Subscript:
                    var current = self.Atoms[index.AtomIndex];
                    if (current.Subscript == null)
                        throw new SubIndexTypeMismatchException(index);
                    current.Subscript.RemoveAt(ref index.SubIndex);
                    break;
                case MathListSubIndexType.Superscript:
                    current = self.Atoms[index.AtomIndex];
                    if (current.Superscript == null)
                        throw new SubIndexTypeMismatchException(index);
                    current.Superscript.RemoveAt(ref index.SubIndex);
                    break;
                default:
                    throw new SubIndexTypeMismatchException(index);
            }
            if (index.Previous is null && index.SubIndexType != MathListSubIndexType.None)
            {
                // We have deleted to the beginning of the line and it is not the outermost line
                if (self.AtomAt(index) is null)
                {
                    self.InsertAndAdvance(ref index, MathAtoms.Placeholder, MathListSubIndexType.None);
                    index = index.Previous ?? throw new InvalidCodePathException("Cannot go back after insertion?"); ;
                }
            }
        }

        public static void RemoveAtoms(this MathList self, MathListRange? nullableRange)
        {
            if (!(nullableRange is MathListRange range)) return;
            var start = range.Start;
            switch (start.SubIndexType)
            {
                case MathListSubIndexType.None:
                    self.RemoveAtoms(new Range(start.AtomIndex, range.Length));
                    break;
                case var _ when start.SubIndex is null:
                    throw new InvalidCodePathException("start.SubIndex is null despite non-None subindex type");
                case MathListSubIndexType.BetweenBaseAndScripts:
                    throw new NotSupportedException("Nuclear fission is not supported");
                case MathListSubIndexType.Radicand:
                case MathListSubIndexType.Degree:
                    if (!(self.Atoms[start.AtomIndex] is Atoms.Radical radical))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), start);
                    if (start.SubIndexType == MathListSubIndexType.Degree)
                        if (radical.Degree is null)
                            throw new SubIndexTypeMismatchException(start);
                        else radical.Degree.RemoveAtoms(range.SubIndexRange);
                    else radical.Radicand.RemoveAtoms(range.SubIndexRange);
                    break;
                case MathListSubIndexType.Numerator:
                case MathListSubIndexType.Denominator:
                    if (!(self.Atoms[start.AtomIndex] is Atoms.Fraction frac))
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), start);
                    if (start.SubIndexType == MathListSubIndexType.Numerator)
                        if (frac.Numerator is null) throw new SubIndexTypeMismatchException(start);
                        else frac.Numerator.RemoveAtoms(range.SubIndexRange);
                    else if (frac.Denominator is null) throw new SubIndexTypeMismatchException(start);
                    else frac.Denominator.RemoveAtoms(range.SubIndexRange);
                    break;
                case MathListSubIndexType.Subscript:
                    var current = self.Atoms[start.AtomIndex];
                    if (current.Subscript == null) throw new SubIndexTypeMismatchException(start);
                    current.Subscript.RemoveAtoms(range.SubIndexRange);
                    break;
                case MathListSubIndexType.Superscript:
                    current = self.Atoms[start.AtomIndex];
                    if (current.Superscript == null) throw new SubIndexTypeMismatchException(start);
                    current.Superscript.RemoveAtoms(range.SubIndexRange);
                    break;
            }
        }

        public static MathAtom? AtomAt(this MathList self, MathListIndex? index)
        {
            if (index is null || index.AtomIndex >= self.Atoms.Count) return null;
            var atom = self.Atoms[index.AtomIndex];
            switch (index.SubIndexType)
            {
                case MathListSubIndexType.None:
                    return atom;
                case var _ when index.SubIndex is null:
                    throw new InvalidCodePathException("index.SubIndex is null despite non-None subindex type");
                case MathListSubIndexType.BetweenBaseAndScripts:
                    return null;
                case MathListSubIndexType.Subscript:
                    return atom.Subscript?.AtomAt(index.SubIndex);
                case MathListSubIndexType.Superscript:
                    return atom.Superscript?.AtomAt(index.SubIndex);
                case MathListSubIndexType.Radicand:
                case MathListSubIndexType.Degree:
                    return
                      atom is Atoms.Radical radical
                      ? index.SubIndexType == MathListSubIndexType.Degree
                        ? radical.Degree?.AtomAt(index.SubIndex)
                        : radical.Radicand?.AtomAt(index.SubIndex)
                      : null;
                case MathListSubIndexType.Numerator:
                case MathListSubIndexType.Denominator:
                    return
                      atom is Atoms.Fraction frac
                      ? index.SubIndexType == MathListSubIndexType.Denominator
                        ? frac.Denominator?.AtomAt(index.SubIndex)
                        : frac.Numerator?.AtomAt(index.SubIndex)
                      : null;
                default:
                    throw new SubIndexTypeMismatchException(index);
            }
        }
    }
}
