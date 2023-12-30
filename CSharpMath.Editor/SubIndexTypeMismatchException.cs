using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  public class SubIndexTypeMismatchException : InvalidOperationException {
    public SubIndexTypeMismatchException(Type atomType, MathListIndex index) : base(
      $"{atomType} not found at index {index.AtomIndex}.") { }
    public SubIndexTypeMismatchException(MathListIndex index) : base(
      $"{index.SubIndexType} not found at index {index.AtomIndex}.") { }
  }
}
