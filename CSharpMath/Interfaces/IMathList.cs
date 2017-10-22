using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IMathList {
    List<IMathAtom> Atoms { get; }

    void AddAtom(IMathAtom atom);
    void InsertAtom(IMathAtom atom, int index);
    void Append(IMathList list);
    void RemoveLastAtom();
    void RemoveAtom(int index);
    void RemoveAtoms(Range inRange);
    
    ///<summary>Not the LaTeX form.</summary>
    string StringValue { get; }

    IMathList FinalizedList();

    IMathList DeepCopy();
  }
}
