using System;
using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph> {
    public TypesettingContext(IFontMeasurer<TGlyph> fontMeasurer, 
      Func<TMathFont, float, TMathFont> mathFontCloner,
      IGlyphBoundsProvider<TMathFont, TGlyph> glyphBoundsProvider,
      IGlyphNameProvider<TGlyph> glyphNameProvider,
      IGlyphFinder<TGlyph> glyphFinder,
      JToken mathJson) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathJson = mathJson;
      MathFontCloner = mathFontCloner;
      GlyphFinder = glyphFinder;
      GlyphNameProvider = glyphNameProvider;
      MathTable = new FontMathTable<TGlyph>(fontMeasurer, mathJson, glyphNameProvider);
    }
    public IFontMeasurer<TGlyph> FontMeasurer { get; }
    public IGlyphBoundsProvider<TMathFont, TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TGlyph> GlyphFinder { get; }
    public IGlyphNameProvider<TGlyph> GlyphNameProvider { get; private set; }

    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathJson { get; }
    public FontMathTable<TGlyph> MathTable { get; private set; }
    public Func<TMathFont, float, TMathFont> MathFontCloner { get; private set; }
  }
}
