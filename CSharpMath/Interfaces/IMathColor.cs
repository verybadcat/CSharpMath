using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IColor : IMathAtom {
    string ColorString { get; set; }
    IMathList InnerList { get; set; }
  }
}
