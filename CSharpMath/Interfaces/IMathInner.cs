using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IMathInner : IMathAtom {
    IMathList InnerList { get; set; }
    IMathAtom LeftBoundary { get; set; }
    IMathAtom RightBoundary { get; set; }
  }
}
