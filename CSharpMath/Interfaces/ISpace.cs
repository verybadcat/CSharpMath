using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface ISpace: IMathAtom {
    float Length { get; }
    bool IsMu { get; }
  }
}
