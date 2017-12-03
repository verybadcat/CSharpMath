using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext<TGlyph> {
    public TypesettingContext(IFontMeasurer fontMeasurer, IGlyphBoundsProvider<TGlyph> glyphBoundsProvider,
      IGlyphFinder<TGlyph> glyphFinder,
      JToken mathJson) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathJson = mathJson;
      GlyphFinder = glyphFinder;
      MathTable = new FontMathTable(fontMeasurer, mathJson);
    }
    public IFontMeasurer FontMeasurer { get; }
    public IGlyphBoundsProvider<TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TGlyph> GlyphFinder { get; }
    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathJson { get; }
    public FontMathTable MathTable { get; private set; }
  }
}
