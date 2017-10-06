using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IInner : IMathAtom {
    IMathList InnerList { get; set; }
    IMathAtom LeftBoundary { get; set; }
    IMathAtom RightBoundary { get; set; }
  }
}
