namespace CSharpMath.Atom
 {
  /// <summary>
  /// A left atom -- Left and Right in TeX.
  /// We don't need two since we track boundaries separately.
  /// Boundaries are invalid inside MathLists, so they do not inherit from MathAtom.
  /// </summary>
  public class Boundary : IMathObject {
    public string Nucleus { get; }
    public string DebugString => Nucleus;
    public Boundary(string nucleus) => Nucleus = nucleus;
    public bool EqualsBoundary(Boundary boundary) => Nucleus == boundary.Nucleus;
    public override bool Equals(object obj) => obj is Boundary b ? EqualsBoundary(b) : false;
    public override int GetHashCode() => unchecked((Nucleus.GetHashCode() * 314 + 89) ^ 0x7E238312);
  }
}