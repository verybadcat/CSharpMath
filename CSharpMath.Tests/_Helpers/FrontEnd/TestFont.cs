namespace CSharpMath.Tests.FrontEnd {
  public readonly struct TestFont : CSharpMath.FrontEnd.IFont<char> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}