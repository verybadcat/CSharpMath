using System;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {
  public class TypesettingContext<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    public TypesettingContext(IFontMeasurer<TFont, TGlyph> fontMeasurer, 
      Func<TFont, float, TFont> mathFontCloner,
      IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider,
      IGlyphNameProvider<TGlyph> glyphNameProvider,
      IGlyphFinder<TGlyph> glyphFinder,
      IFontChanger fontChanger,
      JToken mathJson) {
      FontMeasurer = fontMeasurer;
      GlyphBoundsProvider = glyphBoundsProvider;
      MathJson = mathJson;
      MathFontCloner = mathFontCloner;
      GlyphFinder = glyphFinder;
      GlyphNameProvider = glyphNameProvider;
      FontChanger = fontChanger;
      MathTable = new FontMathTable<TFont, TGlyph>(fontMeasurer, mathJson, glyphNameProvider);
    }
    public IFontMeasurer<TFont, TGlyph> FontMeasurer { get; }
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TGlyph> GlyphFinder { get; }
    public IGlyphNameProvider<TGlyph> GlyphNameProvider { get; private set; }

    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    public JToken MathJson { get; }
    public FontMathTable<TFont, TGlyph> MathTable { get; private set; }
    public Func<TFont, float, TFont> MathFontCloner { get; private set; }
    public IFontChanger FontChanger { get; private set; }
  }
}
