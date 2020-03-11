namespace CSharpMath.Structures {
  /// <summary>
  /// The exception that is thrown when an invalid code path was encountered.
  /// If an instance of this type is thrown, you must have encountered a bug.
  /// Please contact the CSharpMath maintainers.
  /// </summary>
  public class InvalidCodePathException : System.Exception {
    private InvalidCodePathException() { }
    public InvalidCodePathException(string why) : base(why) { }
    public InvalidCodePathException(string why, System.Exception inner) : base(why, inner) { }
  }
}
