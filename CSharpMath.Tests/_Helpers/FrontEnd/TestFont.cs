namespace CSharpMath.Tests.FrontEnd {
  public readonly struct TestFont : Displays.IFont<char> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}