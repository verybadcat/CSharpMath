using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display {
  public class TextLineDisplay {
    public TextLineDisplay(List<TextRunDisplay> runs,
      List<IMathAtom> atoms) {
      Runs = runs;
      Atoms = atoms;
    }
    public List<TextRunDisplay> Runs { get; }
    public List<IMathAtom> Atoms { get; }
  }
}
