namespace CSharpMath.Editor {
  ///<summary>The type of the subindex denotes what branch the path to the atom that this index points to takes.</summary>
  public enum MathListSubIndexType : byte {
    ///<summary>The index denotes the whole atom, subIndex is nil.</summary>
    None = 0,
    ///<summary>The position in the subindex is an index into the nucleus, must be 1</summary>
    BetweenBaseAndScripts,
    ///<summary>The subindex indexes into the superscript.</summary>
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
    Degree
  }

  public class MathListIndex : IMathListIndex<MathListIndex> {
    private MathListIndex() { }

    /// The index of the associated atom.
    public int AtomIndex { get; set; }
    /// The type of subindex, e.g. superscript, numerator etc.
    public MathListSubIndexType SubIndexType { get; set; }
    /// The index into the sublist.
    [NullableReference]
    public MathListIndex SubIndex;

    /** <summary>Factory function to create a `MTMathListIndex` with no subindexes.</summary>
        <param name="index">The index of the atom that the `MTMathListIndex` points at.</param>
     */
    public static MathListIndex Level0Index(int index) => new MathListIndex { AtomIndex = index };
    /** <summary>Factory function to create at `MTMathListIndex` with a given subIndex.</summary>
        <param name="location">The location at which the subIndex should is present.</param>
        <param name="subIndex">The subIndex to be added. Can be nil.</param> 
        <param name="type">The type of the subIndex.</param> 
     */
    public static MathListIndex IndexAtLocation(int location, MathListSubIndexType type, [NullableReference]MathListIndex subIndex) =>
      new MathListIndex { AtomIndex = location, SubIndexType = type, SubIndex = subIndex };

    /** Creates a new index by attaching this index at the end of the current one. */
    public MathListIndex LevelUpWithSubIndex(MathListSubIndexType type, [NullableReference]MathListIndex subIndex) {
      if (SubIndexType is MathListSubIndexType.None)
        return IndexAtLocation(AtomIndex, type, subIndex);
      // we have to recurse
      return IndexAtLocation(AtomIndex, SubIndexType, SubIndex.LevelUpWithSubIndex(type, subIndex));
    }
    /** Creates a new index by removing the last index item. If this is the last one, then returns nil. */
    [NullableReference]
    public MathListIndex LevelDown() {
      if (SubIndexType is MathListSubIndexType.None) {
        return null;
      }
      // recurse
      return SubIndex?.LevelDown() is MathListIndex subIndex ? IndexAtLocation(AtomIndex, SubIndexType, subIndex) : Level0Index(AtomIndex);
    }

    /// Returns the previous index if present. Returns `nil` if there is no previous index.
    [NullableReference]
    public MathListIndex Previous {
      get {
        if (SubIndexType == MathListSubIndexType.None) {
          if (AtomIndex > 0) {
            return Level0Index(AtomIndex - 1);
          }
        } else {
          var prevSubIndex = SubIndex?.Previous;
          if (prevSubIndex != null) {
            return IndexAtLocation(AtomIndex, SubIndexType, prevSubIndex);
          }
        }
        return null;
      }
    }

    /// Returns the next index.
    public MathListIndex Next {
      get {
        if (SubIndexType == MathListSubIndexType.None) {
          return Level0Index(AtomIndex + 1);
        } else if (SubIndexType == MathListSubIndexType.BetweenBaseAndScripts) {
          return IndexAtLocation(AtomIndex + 1, SubIndexType, SubIndex);
        } else {
          return IndexAtLocation(AtomIndex, SubIndexType, SubIndex?.Next);
        }
      }
    }

    /** Returns true if any of the subIndexes of this index have the given type. */
    public bool HasSubIndexOfType(MathListSubIndexType subIndexType) {
      if (SubIndexType == subIndexType) {
        return true;
      } else if (SubIndex != null) {
        return SubIndex.HasSubIndexOfType(subIndexType);
      } else return false;
    }

    /** <summary>
     * Returns true if this index represents the beginning of a line. Note there may be multiple lines in a MTMathList,
     * e.g. a superscript or a fraction numerator. This returns true if the innermost subindex points to the beginning of a
     * line.</summary>
     */
    public bool AtBeginningOfLine => FinalIndex is 0;
    
    public bool AtSameLevel(MathListIndex other) {
      if (SubIndexType != other.SubIndexType) {
        return false;
      } else if (SubIndexType == MathListSubIndexType.None) {
        // No subindexes, they are at the same level.
        return true;
      } else if (AtomIndex != other.AtomIndex) {
        // the subindexes are used in different atoms
        return false;
      } else if (SubIndex != null && other.SubIndex != null) {
        return SubIndex.AtSameLevel(other.SubIndex);
      }
      // No subindexes, they are at the same level.
      return true;
    }

    public int FinalIndex =>
      SubIndexType is MathListSubIndexType.None || SubIndex is null ?
      AtomIndex :
      SubIndex.FinalIndex;

    /** Returns the type of the innermost sub index. */
    public MathListSubIndexType FinalSubIndexType =>
      SubIndex?.SubIndex is null ?
      SubIndexType :
      SubIndex.FinalSubIndexType;

    public MathListIndex FinalSubIndexParent =>
      SubIndex?.SubIndex is null ?
      this :
      SubIndex.FinalSubIndexParent;

    public override string ToString() =>
      SubIndex is null ?
      $@"[{AtomIndex}]" :
      $@"[{AtomIndex}, {SubIndexType}:{SubIndex.ToString().Trim('[', ']')}]";

    public bool EqualsToIndex(MathListIndex index) {
      if (index is null)
        return false;
      if (AtomIndex != index.AtomIndex || SubIndexType != index.SubIndexType) {
        return false;
      }
      if (SubIndex != null && index.SubIndex != null) {
        return SubIndex.EqualsToIndex(index.SubIndex);
      } else {
        return index.SubIndex == null;
      }
    }

    public override bool Equals(object obj) =>
      obj is MathListIndex index && EqualsToIndex(index);
    public override int GetHashCode() =>
      unchecked((AtomIndex * 31 + (int)SubIndexType) * 31 + SubIndex.GetHashCode());
  }
}