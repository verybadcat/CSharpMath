namespace CSharpMath.CoreTests.FrontEnd {
  public readonly struct TestFont : Display.FrontEnd.IFont<System.Text.Rune> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}