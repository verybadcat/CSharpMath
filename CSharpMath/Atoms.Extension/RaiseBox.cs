using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms.Extension {
  public class RaiseBox : _ExtensionAtom<RaiseBox> {
    public Space Raise { get; set; }
    public IMathList InnerList { get; set; }
    public RaiseBox() : base(MathAtomType.RaiseBox) { }
    /*public RaiseBox(RaiseBox cloneMe, bool finalize) : base(cloneMe, finalize) =>
      InnerList = cloneMe.InnerList;*/

    protected override void CopyPropertiesFrom(RaiseBox oldAtom) {
      InnerList = oldAtom.InnerList;
      Raise = oldAtom.Raise;
    }
  }
}
