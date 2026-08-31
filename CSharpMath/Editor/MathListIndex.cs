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
    Inner,
    /// <summary>The subindex selects a row in a table.</summary>
    TableRow,
    /// <summary>The subindex selects a cell in a table row.</summary>
    TableColumn,
    /// <summary>The subindex is the caret index within a table cell.</summary>
    TableCell
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
  public record class MathListIndex(int AtomIndex, (MathListSubIndexType SubIndexType, MathListIndex SubIndex)? SubIndexInfo = null) {
    /// <summary>Creates an index into a table cell while retaining its row and column.</summary>
    public MathListIndex TableCell(int row, int column, MathListIndex cellIndex) =>
      new(AtomIndex, (MathListSubIndexType.TableRow,
        new(row, (MathListSubIndexType.TableColumn,
          new(column, (MathListSubIndexType.TableCell, cellIndex))))));
    /// <summary>
    /// Creates a new MathListIndex that represents a subindex within this list, wrapped at the specified outer atom
    /// index and subindex type.
    /// </summary>
    /// <param name="outerAtomIndex">The zero-based index of the outer atom in the list at which to wrap the subindex.</param>
    /// <param name="type">The type of subindex to create, specifying the relationship to the outer atom.</param>
    /// <returns>A <see cref="MathListIndex"/> representing the specified subindex within this list.</returns>
    public MathListIndex Wrap(int outerAtomIndex, MathListSubIndexType type) => new(outerAtomIndex, (type, this));

    ///<summary>Creates a new index by replacing the leaf with IndexInfo (type, new(innerAtomIndex)).</summary>
    public MathListIndex LevelUpWithSubIndex(MathListSubIndexType type, int innerAtomIndex) =>
      SubIndexInfo switch {
        null => new(AtomIndex, (type, new(innerAtomIndex))),
        var (thisType, thisSubIndex) => new(AtomIndex, (thisType, thisSubIndex.LevelUpWithSubIndex(type, innerAtomIndex)))
      };
    ///<summary>Creates a new index by removing the last index item. If this is the last one, then returns <see langword="null"/>.</summary>
    public MathListIndex? LevelDown() =>
      SubIndexInfo switch {
        null => null,
        var (type, subIndex) => subIndex.LevelDown() is { } levelledDownSubIndex ? new(AtomIndex, (type, levelledDownSubIndex)) : new(AtomIndex)
      };

    /** <summary>
     * Returns the previous index if this index is not at the beginning of a line.
     * Note there may be multiple lines in a MathList,
     * e.g. a superscript or a fraction numerator.
     * This returns <see langword="null"/> if there is no previous index, i.e.
     * the innermost subindex points to the beginning of a line.</summary>
     */
    public MathListIndex? Previous => SubIndexInfo switch {
      null => AtomIndex > 0 ? new(AtomIndex - 1) : null,
      var (type, subIndex) => subIndex.Previous is { } prevSubIndex ? new(AtomIndex, (type, prevSubIndex)) : null,
    };

    /// <summary>Returns the next index. With the exception of BetweenBaseAndScripts, this adds 1 to the AtomIndex of the leaf.</summary>
    public MathListIndex Next => SubIndexInfo switch {
      null => new(AtomIndex + 1),
      (MathListSubIndexType.BetweenBaseAndScripts, _) => new(AtomIndex + 1, SubIndexInfo),
      var (type, subIndex) => new(AtomIndex, (type, subIndex.Next))
    };

    /// <summary>Returns true if any of the subIndexes of this index have the given type.</summary>
    public bool HasSubIndexOfType(MathListSubIndexType subIndexType) =>
      SubIndexInfo switch {
        null => false,
        var (type, subIndex) => subIndexType == type || subIndex.HasSubIndexOfType(subIndexType)
      };
    /// <summary>Same, or differing only with respect to the final AtomIndex.</summary>
    public bool AtSameLevel(MathListIndex other) =>
      (SubIndexInfo, other.SubIndexInfo) switch {
        (null, null) => true,
        ((_, _), null) => false,
        (null, (_, _)) => false,
        (var (aType, aIndex), var (bType, bIndex)) =>
          aType == bType && AtomIndex == other.AtomIndex && aIndex.AtSameLevel(bIndex)
      };

    public int FinalIndex =>
      SubIndexInfo switch {
        null => AtomIndex,
        var (_, subIndex) => subIndex.FinalIndex
      };

    ///<summary>Returns the type of the innermost sub index.</summary>
    public MathListSubIndexType? FinalSubIndexType =>
      SubIndexInfo switch {
        null => null,
        var (type, subIndex) => subIndex.SubIndexInfo is null ? type : subIndex.FinalSubIndexType
      };

    public override string ToString() =>
      SubIndexInfo switch {
        null => $@"[{AtomIndex}]",
        var (type, subIndex) => $@"[{AtomIndex}, {type}:{subIndex.ToString().Trim('[', ']')}]"
      };
  }
}
