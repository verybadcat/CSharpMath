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

    int AtomIndex { get; }
    MathListSubIndexType SubIndexType { get; }
    //[NullableReference] TSelf SubIndex { get; }

    [NullableReference] TSelf Previous { get; }
    TSelf Next { get; }

    bool AtBeginningOfLine { get; }

    MathListSubIndexType FinalSubIndexType { get; }

    bool HasSubIndexOfType(MathListSubIndexType subIndexType);

    TSelf LevelUpWithSubIndex(MathListSubIndexType type, [NullableReference] TSelf subIndex);
    [NullableReference] TSelf LevelDown();

    bool Equals([NullableReference] object o);
    int GetHashCode();
    string ToString();

    //[StaticInterfaceMembers]
#if false
    static TSelf Level0Index(uint index);

    static TSelf IndexAtLocation(uint location, [NullableReference] IMathListIndex subIndex, MathListSubIndexType type);
#endif
  }
}