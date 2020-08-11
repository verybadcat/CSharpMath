namespace CSharpMath.Atom {
  /// <summary>Marker interface</summary>
  public interface IMathObject {
    string DebugString { get; }
  }
  public interface IMathListContainer : IMathObject {
    System.Collections.Generic.IEnumerable<MathList> InnerLists { get; }
  }
}