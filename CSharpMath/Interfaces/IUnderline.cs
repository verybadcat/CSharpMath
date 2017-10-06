using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IUnderline: IMathAtom {
    IMathList InnerList { get; set; }
  }
}
