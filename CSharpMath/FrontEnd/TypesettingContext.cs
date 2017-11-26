using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext {
    public TypesettingContext(IFontMeasurer fontMeasurer, IGlyphBoundsProvider glyphBoundsProvider, JToken mathJson) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathJson = mathJson;
      MathTable = new FontMathTable(fontMeasurer, mathJson);
    }
    public IFontMeasurer FontMeasurer { get; }
    public IGlyphBoundsProvider GlyphBoundsProvider { get; }
    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathJson { get; }
    public FontMathTable MathTable { get; private set; }
  }
}
