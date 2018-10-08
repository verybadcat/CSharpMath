using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  public class SubIndexTypeMismatchException : InvalidOperationException {
    public SubIndexTypeMismatchException() { }
    public SubIndexTypeMismatchException(string message) : base(message) { }
    public SubIndexTypeMismatchException(string message, Exception inner) : base(message, inner) { }
  }
}
