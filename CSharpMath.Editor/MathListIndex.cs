namespace CSharpMath.Editor {
  /**
   @typedef MTMathListSubIndexType
   @brief The type of the subindex.
   
   The type of the subindex denotes what branch the path to the atom that this index points to takes.
   */
  public enum MathListSubIndexType : byte {
    /// The index denotes the whole atom, subIndex is nil.
    None = 0,
    /// The position in the subindex is an index into the nucleus
    Nucleus,
    /// The subindex indexes into the superscript.
    Superscript,
    /// The subindex indexes into the subscript
    Subscript,
    /// The subindex indexes into the numerator (only valid for fractions)
    Numerator,
    /// The subindex indexes into the denominator (only valid for fractions)
    Denominator,
    /// The subindex indexes into the radicand (only valid for radicals)
    Radicand,
    /// The subindex indexes into the degree (only valid for radicals)
    Degree
  }

  public class MathListIndex : IMathListIndex<MathListIndex> {
    private MathListIndex() { }

    public int AtomIndex { get; set; }
    public MathListSubIndexType SubIndexType { get; set; }
    [NullableReference]
    public MathListIndex SubIndex { get; set; }

    public static MathListIndex Level0Index(int index) => new MathListIndex { AtomIndex = index };
    public static MathListIndex IndexAtLocation(int location, MathListSubIndexType type, [NullableReference]MathListIndex subIndex) =>
      new MathListIndex { AtomIndex = location, SubIndexType = type, SubIndex = subIndex };

    public MathListIndex LevelUpWithSubIndex(MathListSubIndexType type, [NullableReference]MathListIndex subIndex) {
      if (SubIndexType is MathListSubIndexType.None)
        return IndexAtLocation(AtomIndex, type, subIndex);
      // we have to recurse
      return IndexAtLocation(AtomIndex, SubIndexType, SubIndex.LevelUpWithSubIndex(type, subIndex));
    }
    [NullableReference]
    public MathListIndex LevelDown() {
      if (SubIndexType is MathListSubIndexType.None) {
        return null;
      }
      // recurse
      return SubIndex?.LevelDown() is MathListIndex subIndex ? IndexAtLocation(AtomIndex, SubIndexType, subIndex) : Level0Index(AtomIndex);
    }

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

    public MathListIndex Next {
      get {
        if (SubIndexType == MathListSubIndexType.None) {
          return Level0Index(AtomIndex + 1);
        } else if (SubIndexType == MathListSubIndexType.Nucleus) {
          return IndexAtLocation(AtomIndex + 1, SubIndexType, SubIndex);
        } else {
          return IndexAtLocation(AtomIndex, SubIndexType, SubIndex?.Next);
        }
      }
    }

    public bool HasSubIndexOfType(MathListSubIndexType subIndexType) {
      if (SubIndexType == subIndexType) {
        return true;
      } else if (SubIndex != null) {
        return SubIndex.HasSubIndexOfType(subIndexType);
      } else return false;
    }

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
      $@"[{AtomIndex}, {SubIndexType}:{SubIndex}]";

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