using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IOverline: IMathAtom {
    IMathList InnerList { get; set; }
  }
}
