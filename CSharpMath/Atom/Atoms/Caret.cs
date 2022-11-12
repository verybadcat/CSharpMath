using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Atom.Atoms {
  public sealed class Caret : MathAtom {

    public Colored CartList => new Colored(LaTeXSettings.CertColor,LaTeXSettings.CertList);
    public override bool ScriptsAllowed => false;
    public override string DebugString => CartList.DebugString;
    protected override MathAtom CloneInside(bool finalize) => new Caret();
  }
  
}
