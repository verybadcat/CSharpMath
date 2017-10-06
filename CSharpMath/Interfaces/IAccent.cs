using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IAccent : IMathAtom {
    IMathList InnerList { get; set; }
  }
}
