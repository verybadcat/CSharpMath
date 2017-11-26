using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext {
    public TypesettingContext(IFontMeasurer fontMeasurer, IGlyphBoundsProvider glyphBoundsProvider, JToken mathTable) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathTable = mathTable;
    }
    public IFontMeasurer FontMeasurer { get; }
    public IGlyphBoundsProvider GlyphBoundsProvider { get; }
    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathTable { get; }
  }
}
