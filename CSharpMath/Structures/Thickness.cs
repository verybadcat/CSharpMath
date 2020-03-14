namespace CSharpMath.Structures {
  public readonly struct Thickness : System.IEquatable<Thickness> {
    public Thickness(float uniformSize) { Left = Right = Top = Bottom = uniformSize; }
    public Thickness(float horizontalSize, float verticalSize)
      { Left = Right = horizontalSize; Top = Bottom = verticalSize; }
    public Thickness(float left, float top, float right, float bottom)
      { Left = left; Top = top; Right = right; Bottom = bottom; }
    public float Top { get; }
    public float Bottom { get; }
    public float Left { get; }
    public float Right { get; }
    public void Deconstruct(out float left, out float top, out float right, out float bottom) =>
      (left, top, right, bottom) = (Left, Top, Right, Bottom);
    public static bool operator ==(Thickness left, Thickness right) =>
      (left.Top, left.Bottom, left.Left, left.Right) ==
      (right.Top, right.Bottom, right.Left, right.Right);
    public static bool operator !=(Thickness left, Thickness right) => !(left == right);
    public override bool Equals(object obj) => obj is Thickness t && this == t;
    public bool Equals(Thickness other) => this == other;
    public override int GetHashCode() =>
      unchecked(Top.GetHashCode() * 7717 + Bottom.GetHashCode() * 1777
              + Left.GetHashCode() * 71 + Right.GetHashCode());
  }
}
