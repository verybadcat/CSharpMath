namespace CSharpMath.Tests.FrontEnd {
  public readonly struct TestFont : Display.IFont<char> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}
