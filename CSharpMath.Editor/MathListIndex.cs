using System;
using System.Text.RegularExpressions;

namespace CSharpMath.Editor {
  ///<summary>The type of the subindex denotes what branch the path to the atom that this index points to takes.</summary>
  public enum MathListSubIndexType : byte {
    ///<summary>The position in the subindex is an index into the nucleus, must be 1</summary>
    BetweenBaseAndScripts,
    ///<summary>The subindex indexes into the superscript</summary>
    Superscript,
    ///<summary>The subindex indexes into the subscript</summary>
    Subscript,
    ///<summary>The subindex indexes into the numerator (only valid for fractions)</summary>
    Numerator,
    ///<summary>The subindex indexes into the denominator (only valid for fractions)</summary>
    Denominator,
    ///<summary>The subindex indexes into the radicand (only valid for radicals)</summary>
    Radicand,
    ///<summary>The subindex indexes into the degree (only valid for radicals)</summary>
    Degree,
    ///<summary>The subindex indexes into the inner list (only valid for inners)</summary>
    Inner
  }

  /** <summary>
* An index that points to a particular character in the MathList. The index is a LinkedList that represents
* a path from the beginning of the MathList to reach a particular atom in the list. The next node of the path
* is represented by the subIndex. The path terminates when the subIndex is nil.
*
* If there is a subIndex, the subIndexType denotes what branch the path takes (i.e. superscript, subscript, 
* numerator, denominator etc.).
* e.g in the expression 25^{2/4} the index of the character 4 is represented as:
* (1, superscript) -> (0, denominator) -> (0, none)
* This can be interpreted as start at index 1 (i.e. the 5) go up to the superscript.
* Then look at index 0 (i.e. 2/4) and go to the denominator. Then look up index 0 (i.e. the 4) which this final
* index.
* 
* The level of an index is the number of nodes in the LinkedList to get to the final path.
* </summary>*/
  public class MathListIndex {

    ///<summary>The index of the associated atom.</summary>
    public int AtomIndex { get; set; }

    public (MathListSubIndexType SubIndexType, MathListIndex SubIndex)? SubIndexInfo;

    public void ReplaceWith(MathListIndex replacement) {
      AtomIndex = replacement.AtomIndex;
      SubIndexInfo = replacement.SubIndexInfo;
    }

    /** <summary>Factory function to create a `MathListIndex` with no subindexes.</summary>
        <param name="index">The index of the atom that the `MathListIndex` points at.</param>
     */
    public static MathListIndex Level0Index(int index) => new MathListIndex(index, null);
    public MathListIndex(int atomIndex, (MathListSubIndexType SubIndexType, MathListIndex SubIndex)? subIndexInfo) {
      AtomIndex = atomIndex;
      SubIndexInfo = subIndexInfo;
    }

    ///<summary>Creates a new index by attaching this index at the end of the current one.</summary>
    public MathListIndex LevelUpWithSubIndex(MathListSubIndexType type, MathListIndex subIndex) =>
      SubIndexInfo switch
      {
        null => new MathListIndex(AtomIndex, (type, subIndex)),
        (MathListSubIndexType thisType, MathListIndex thisSubIndex) =>
          new MathListIndex(AtomIndex,
            (thisType, thisSubIndex.LevelUpWithSubIndex(type, subIndex)))
      };
    ///<summary>Creates a new index by removing the last index item. If this is the last one, then returns nil.</summary>
    public MathListIndex? LevelDown() =>
      SubIndexInfo switch
      {
        null => null,
        (MathListSubIndexType type, MathListIndex subIndex) =>
          subIndex.LevelDown() is MathListIndex levelledDownSubIndex
          ? new MathListIndex(AtomIndex, (type, levelledDownSubIndex)) :
          Level0Index(AtomIndex)
      };

    /** <summary>
     * Returns the previous index if this index is not at the beginning of a line.
     * Note there may be multiple lines in a MathList,
     * e.g. a superscript or a fraction numerator.
     * This returns <see cref="null"/> if there is no previous index, i.e.
     * the innermost subindex points to the beginning of a line.</summary>
     */
    public MathListIndex? Previous => SubIndexInfo switch
    {
      null => AtomIndex > 0 ? Level0Index(AtomIndex - 1) : null,
      (MathListSubIndexType type, MathListIndex subIndex) =>
        subIndex.Previous is MathListIndex prevSubIndex
        ? new MathListIndex(AtomIndex, (type, prevSubIndex))
        : null,
    };

    ///<summary>Returns the next index.</summary>
    public MathListIndex Next => SubIndexInfo switch
    {
      null => Level0Index(AtomIndex + 1),
      (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex _) =>
        new MathListIndex(AtomIndex + 1, SubIndexInfo),
      (MathListSubIndexType subIndexType, MathListIndex index) =>
        new MathListIndex(AtomIndex, (subIndexType, index.Next))
    };

    ///<summary>Returns true if any of the subIndexes of this index have the given type.</summary>
    public bool HasSubIndexOfType(MathListSubIndexType subIndexType) =>
      SubIndexInfo switch
      {
        null => false,
        (MathListSubIndexType type, MathListIndex subIndex) =>
          subIndexType == type || subIndex.HasSubIndexOfType(subIndexType)
      };
    /// <summary>Same, or differing only with respect to the final AtomIdex.</summary>
    public bool AtSameLevel(MathListIndex other) =>
      (SubIndexInfo, other.SubIndexInfo) switch
      {
        (null, null) => true,
        ((_, _), null) => false,
        (null, (_, _)) => false,
        ((MathListSubIndexType aType, MathListIndex aIndex), (MathListSubIndexType bType, MathListIndex bIndex)) =>
          aType == bType && AtomIndex == other.AtomIndex && aIndex.AtSameLevel(bIndex)
      };

    public int FinalIndex =>
      SubIndexInfo switch
      {
        null => AtomIndex,
        (_, MathListIndex subIndex) => subIndex.FinalIndex
      };

    ///<summary>Returns the type of the innermost sub index.</summary>
    public MathListSubIndexType? FinalSubIndexType =>
      SubIndexInfo switch
      {
        null => null,
        (MathListSubIndexType type, MathListIndex subIndex) =>
          subIndex.SubIndexInfo == null ? type : subIndex.FinalSubIndexType
      };

    public override string ToString() =>
      SubIndexInfo switch
      {
        null => $@"[{AtomIndex}]",
        (MathListSubIndexType type, MathListIndex subIndex) =>
          $@"[{AtomIndex}, {type}:{subIndex.ToString().Trim('[', ']')}]"
      };

    public bool EqualsToIndex(MathListIndex other) =>
      (SubIndexInfo, other.SubIndexInfo) switch
      {
        (null, null) => true,
        ((_, _), null) => false,
        (null, (_, _)) => false,
        ((MathListSubIndexType aType, MathListIndex aIndex), (MathListSubIndexType bType, MathListIndex bIndex)) =>
          aType == bType && aIndex.EqualsToIndex(bIndex)
      };

    public override bool Equals(object obj) =>
      obj is MathListIndex index && EqualsToIndex(index);
    public override int GetHashCode() => unchecked(
      SubIndexInfo switch
      {
        null => AtomIndex * 31 - 1,
        (MathListSubIndexType type, MathListIndex subIndex) =>
          AtomIndex * 31 +(int)type * 31 + subIndex.GetHashCode()
      });
  }
}