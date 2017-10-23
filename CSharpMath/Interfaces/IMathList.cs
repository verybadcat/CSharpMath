using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  /// <summary>
  /// Noteworthy that this does NOT inherit from IMathAtom.
  /// </summary>
  public interface IMathList: IStringValue {
    List<IMathAtom> Atoms { get; }

    void AddAtom(IMathAtom atom);
    void InsertAtom(IMathAtom atom, int index);
    void Append(IMathList list);
    void RemoveLastAtom();
    void RemoveAtom(int index);
    void RemoveAtoms(Range inRange);
    



    IMathList FinalizedList();

//    IMathList DeepCopy(); // can we get away without this?
  }
}
