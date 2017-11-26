using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  public class TextLineDisplay: DisplayBase {
    public TextLineDisplay(List<TextRunDisplay> runs,
      IEnumerable<IMathAtom> atoms) {
      Runs = runs;
      Atoms = atoms.ToList();
    }
    public List<TextRunDisplay> Runs { get; }
    public List<IMathAtom> Atoms { get; }
    public string Text {
      get {
        string r = "";
        foreach (var run in Runs) {
          r += run.Run.Text;
        }
        return r;
      }
    }
  }
}
