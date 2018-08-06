using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath
{
  /// <summary>
  /// The exception that is thrown when an invalid code path was encountered.
  /// Contact the CSharpMath maintainers if an instance of this type is thrown.
  /// </summary>
  public class InvalidCodePathException : Exception {
    public InvalidCodePathException(string message) : base(message) { }
    public InvalidCodePathException(string message, Exception inner) : base(message, inner) { }
  }
}
