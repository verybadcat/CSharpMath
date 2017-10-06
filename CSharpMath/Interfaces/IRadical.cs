using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IRadical: IMathAtom {
    /// <summary>
    /// Whatever is under the square root sign
    /// </summary>
    IMathList Radicand { get; set; }
    IMathList Degree { get; set; }
  }
}
