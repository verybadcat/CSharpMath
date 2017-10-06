using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IFraction : IMathAtom {
    IMathList Numerator { get; set; }
    IMathList Denominator { get; set;}
    /// <summary>
    /// In this context, a "rule" is a fraction line.
    /// </summary>
    bool HasRule { get; }
    string LeftDelimiter { get; set; }
    string RightDelmiter { get; set; }
  }
}
