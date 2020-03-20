namespace CSharpMath.Atom {
  /// <summary>Marker interface</summary>
  public interface IMathObject {
    string DebugString { get; }
  }
  public interface IMathListContainer1 : IMathObject {
    MathList InnerList { get; }
  }
  public interface IMathListContainer2 : IMathObject {
    MathList InnerList1 { get; }
    MathList InnerList2 { get; }
  }
  public interface IMathListContainerN : IMathObject {
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