using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  /// <summary>
  /// Noteworthy that this does NOT inherit from IMathAtom.
  /// </summary>
  public interface IMathList: IStringValue, IMathObject, IList<IMathAtom> {
    List<IMathAtom> Atoms { get; }

    void Append(IMathList list);
    void RemoveAtoms(Range inRange);

    ///<summary>Iteratively expands all groups in this list.</summary>
    void ExpandGroups();


    IMathList FinalizedList();

//    IMathList DeepCopy(); // can we get away without this?
  }
}
