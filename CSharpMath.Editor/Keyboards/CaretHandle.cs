namespace CSharpMath.Editor {
  public readonly struct CaretHandle {
    public CaretHandle(float fontSize) {
      Width = fontSize / 2;
      Height = fontSize * 2 / 3;
    }
    public float Width { get; }
    public float Height { get; }
  }
}