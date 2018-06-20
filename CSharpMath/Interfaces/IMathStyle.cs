using CSharpMath.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IStyle : IMathAtom {
    LineStyle LineStyle { get; }
  }
}
