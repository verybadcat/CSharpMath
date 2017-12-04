using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext<TGlyph> {
    public TypesettingContext(IFontMeasurer<TGlyph> fontMeasurer, 
      IGlyphBoundsProvider<TGlyph> glyphBoundsProvider,
      IGlyphNameProvider<TGlyph> glyphNameProvider,
      IGlyphFinder<TGlyph> glyphFinder,
      JToken mathJson) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathJson = mathJson;
      GlyphFinder = glyphFinder;
      GlyphNameProvider = glyphNameProvider;
      MathTable = new FontMathTable<TGlyph>(fontMeasurer, mathJson, glyphNameProvider);
    }
    public IFontMeasurer<TGlyph> FontMeasurer { get; }
    public IGlyphBoundsProvider<TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TGlyph> GlyphFinder { get; }
    public IGlyphNameProvider<TGlyph> GlyphNameProvider { get; private set; }

    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathJson { get; }
    public FontMathTable<TGlyph> MathTable { get; private set; }
  }
}
