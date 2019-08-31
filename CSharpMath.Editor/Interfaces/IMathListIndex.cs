using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  /** 
* An index that points to a particular character in the MTMathList. The index is a LinkedList that represents
* a path from the beginning of the MTMathList to reach a particular atom in the list. The next node of the path
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
*/
  interface IMathListIndex<TSelf> where TSelf : IMathListIndex<TSelf> {

    /// The index of the associated atom.
    int AtomIndex { get; }
    /// The type of subindex, e.g. superscript, numerator etc.
    MathListSubIndexType SubIndexType { get; }
    /// The index into the sublist.
    //[NullableReference] TSelf SubIndex { get; }

    /// Returns the previous index if present. Returns `nil` if there is no previous index.
    [NullableReference] TSelf Previous { get; }
    /// Returns the next index.
    TSelf Next { get; }

    /** 
     * Returns true if this index represents the beginning of a line. Note there may be multiple lines in a MTMathList,
     * e.g. a superscript or a fraction numerator. This returns true if the innermost subindex points to the beginning of a
     * line.
     */
    bool AtBeginningOfLine { get; }

    /** Returns the type of the innermost sub index. */
    MathListSubIndexType FinalSubIndexType { get; }

    /** Returns true if any of the subIndexes of this index have the given type. */
    bool HasSubIndexOfType(MathListSubIndexType subIndexType);

    /** Creates a new index by attaching this index at the end of the current one. */
    TSelf LevelUpWithSubIndex(MathListSubIndexType type, [NullableReference] TSelf subIndex);
    /** Creates a new index by removing the last index item. If this is the last one, then returns nil. */
    [NullableReference] TSelf LevelDown();

    bool Equals([NullableReference] object o);
    int GetHashCode();
    string ToString();

    //[StaticInterfaceMembers]
#if false
    /** Factory function to create a `MTMathListIndex` with no subindexes.
        @param index The index of the atom that the `MTMathListIndex` points at.
     */
    static TSelf Level0Index(uint index);

    /** Factory function to create at `MTMathListIndex` with a given subIndex.
        @param location The location at which the subIndex should is present.
        @param subIndex The subIndex to be added. Can be nil.
        @param type The type of the subIndex.
     */
    static TSelf IndexAtLocation(uint location, [NullableReference] IMathListIndex subIndex, MathListSubIndexType type);
#endif
  }
}