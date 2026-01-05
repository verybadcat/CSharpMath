namespace CSharpMath.Core.BackEnd {
  public readonly struct TestFont : Display.FrontEnd.IFont<System.Text.Rune> {
    public TestFont(float pointSize) => PointSize = pointSize;
    public float PointSize { get; }
  }
}