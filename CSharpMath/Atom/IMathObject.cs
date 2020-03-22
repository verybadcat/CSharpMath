namespace CSharpMath.Atom {
  /// <summary>Marker interface</summary>
  public interface IMathObject {
    string DebugString { get; }
  }
  public interface IMathListContainer : IMathObject {
    System.Collections.Generic.IEnumerable<MathList> InnerLists { get; }
  }
}
namespace CSharpMath {
  using Atom;
  partial class Extensions {
    /// Safe to call, even if one or both are null. Returns true if both are null. 
    public static bool NullCheckingStructuralEquality(this IMathObject? obj1, IMathObject? obj2) =>
      obj1 == null ? obj2 == null : obj2 == null ? false : obj1.Equals(obj2);
  }
}