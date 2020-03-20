namespace CSharpMath.Atom {
  /// <summary>
  /// A left atom -- Left and Right in TeX.
  /// We don't need two since we track boundaries separately.
  /// </summary>
  public readonly struct Boundary : IMathObject, System.IEquatable<Boundary> {
    public static readonly Boundary Empty = new Boundary("");
    public string Nucleus { get; }
    public string DebugString => Nucleus;
    public Boundary(string nucleus) => Nucleus = nucleus;
    public bool EqualsBoundary(Boundary boundary) => Nucleus == boundary.Nucleus;
    bool System.IEquatable<Boundary>.Equals(Boundary other) => EqualsBoundary(other);
    public override bool Equals(object obj) => obj is Boundary b ? EqualsBoundary(b) : false;
    public override int GetHashCode() => unchecked((Nucleus.GetHashCode() * 314 + 89) ^ 0x7E238312);
    public static bool operator ==(Boundary left, Boundary right) => left.EqualsBoundary(right);
    public static bool operator !=(Boundary left, Boundary right) => !left.EqualsBoundary(right);
  }
}