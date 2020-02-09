namespace CSharpMath.Apple {
  public class AppleFontMeasurer : FrontEnd.IFontMeasurer<AppleMathFont, ushort> {
    private AppleFontMeasurer() { }
    public static AppleFontMeasurer Instance { get; } = new AppleFontMeasurer();
    public int GetUnitsPerEm(AppleMathFont font) => (int)font.CtFont.UnitsPerEmMetric;
  }
}