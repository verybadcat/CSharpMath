using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  public class SubIndexTypeMismatchException : InvalidOperationException {
    public SubIndexTypeMismatchException(string target, int atomIndex) : base($"{target} not found at index {atomIndex}.") { }
  }
}