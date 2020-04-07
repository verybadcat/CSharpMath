namespace CSharpMath.Editor {
  ///<summary>The type of the subindex denotes what branch the path to the atom that this index points to takes.</summary>
  public enum MathListSubIndexType : byte {
    ///<summary>The index denotes the whole atom, subIndex is null</summary>
    None = 0,
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
    private MathListIndex() { }

    ///<summary>The index of the associated atom.</summary>
    public int AtomIndex { get; set; }
    ///<summary>The type of subindex, e.g. superscript, numerator etc.</summary>
    public MathListSubIndexType SubIndexType { get; set; }
    ///<summary>The index into the sublist.</summary>
    public MathListIndex? SubIndex;

    /** <summary>Factory function to create a `MathListIndex` with no subindexes.</summary>
        <param name="index">The index of the atom that the `MathListIndex` points at.</param>
     */
    public static MathListIndex Level0Index(int index) => new MathListIndex { AtomIndex = index };
    /** <summary>Factory function to create at `MathListIndex` with a given subIndex.</summary>
        <param name="location">The location at which the subIndex should is present.</param>
        <param name="subIndex">The subIndex to be added. Can be nil.</param> 
        <param name="type">The type of the subIndex.</param> 
     */
    public static MathListIndex IndexAtLocation(int location, MathListSubIndexType type, MathListIndex? subIndex) =>
      new MathListIndex { AtomIndex = location, SubIndexType = type, SubIndex = subIndex };

    ///<summary>Creates a new index by attaching this index at the end of the current one.</summary>
    public MathListIndex LevelUpWithSubIndex(MathListSubIndexType type, MathListIndex? subIndex) =>
      SubIndexType is MathListSubIndexType.None ? IndexAtLocation(AtomIndex, type, subIndex) :
      IndexAtLocation(AtomIndex, SubIndexType, SubIndex?.LevelUpWithSubIndex(type, subIndex));
    ///<summary>Creates a new index by removing the last index item. If this is the last one, then returns nil.</summary>
    public MathListIndex? LevelDown() =>
      SubIndexType is MathListSubIndexType.None ? null :
      SubIndex?.LevelDown() is MathListIndex subIndex ? IndexAtLocation(AtomIndex, SubIndexType, subIndex) :
      Level0Index(AtomIndex);

    /** <summary>
     * Returns the previous index if this index is not at the beginning of a line.
     * Note there may be multiple lines in a MathList,
     * e.g. a superscript or a fraction numerator.
     * This returns <see cref="null"/> if there is no previous index, i.e.
     * the innermost subindex points to the beginning of a line.</summary>
     */
    public MathListIndex? Previous => SubIndexType switch
    {
      MathListSubIndexType.None => AtomIndex > 0 ? Level0Index(AtomIndex - 1) : null,
      _ => SubIndex?.Previous is MathListIndex prevSubIndex ? IndexAtLocation(AtomIndex, SubIndexType, prevSubIndex) : null,
    };

    ///<summary>Returns the next index.</summary>
    public MathListIndex Next => SubIndexType switch
    {
      MathListSubIndexType.None => Level0Index(AtomIndex + 1),
      MathListSubIndexType.BetweenBaseAndScripts => IndexAtLocation(AtomIndex + 1, SubIndexType, SubIndex),
      _ => IndexAtLocation(AtomIndex, SubIndexType, SubIndex?.Next),
    };

    ///<summary>Returns true if any of the subIndexes of this index have the given type.</summary>
    public bool HasSubIndexOfType(MathListSubIndexType subIndexType) =>
      SubIndexType == subIndexType ? true :
      SubIndex != null ? SubIndex.HasSubIndexOfType(subIndexType) : false;

    public bool AtSameLevel(MathListIndex other) =>
      SubIndexType != other.SubIndexType ? false :
      // No subindexes, they are at the same level.
      SubIndexType == MathListSubIndexType.None ? true :
      // the subindexes are used in different atoms
      AtomIndex != other.AtomIndex ? false :
      SubIndex != null && other.SubIndex != null ? SubIndex.AtSameLevel(other.SubIndex) :
      // No subindexes, they are at the same level.
      true;

    public int FinalIndex =>
      SubIndexType is MathListSubIndexType.None || SubIndex is null ? AtomIndex : SubIndex.FinalIndex;

    ///<summary>Returns the type of the innermost sub index.</summary>
    public MathListSubIndexType FinalSubIndexType =>
      SubIndex?.SubIndex is null ? SubIndexType : SubIndex.FinalSubIndexType;

    public MathListIndex FinalSubIndexParent =>
      SubIndex?.SubIndex is null ? this : SubIndex.FinalSubIndexParent;

    public override string ToString() =>
      SubIndex is null ?
      $@"[{AtomIndex}]" :
      $@"[{AtomIndex}, {SubIndexType}:{SubIndex.ToString().Trim('[', ']')}]";

    public bool EqualsToIndex(MathListIndex index) =>
      index is null || AtomIndex != index.AtomIndex || SubIndexType != index.SubIndexType ? false :
      SubIndex != null && index.SubIndex != null ? SubIndex.EqualsToIndex(index.SubIndex) :
      index.SubIndex == null;

    public override bool Equals(object obj) =>
      obj is MathListIndex index && EqualsToIndex(index);
    public override int GetHashCode() =>
      unchecked((AtomIndex * 31 + (int)SubIndexType) * 31 + (SubIndex?.GetHashCode() ?? -1));
  }
}