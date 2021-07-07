using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  public class SubIndexTypeMismatchException : InvalidOperationException {
    public SubIndexTypeMismatchException(Type atomType, MathListIndex index) : base(
      $"{atomType} not found at index {index.AtomIndex}.") { }
    public SubIndexTypeMismatchException(MathListIndex index) : base(
      Array.IndexOf(typeof(MathListSubIndexType).GetEnumValues(), index.SubIndexInfo!.Value.SubIndexType) == -1
      ? $"{index.SubIndexInfo!.Value.SubIndexType} is an invalid subindex type."
      : $"{index.SubIndexInfo!.Value.SubIndexType} not found at index {index.AtomIndex}.") { }
  }
}
