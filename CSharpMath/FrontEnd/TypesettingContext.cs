using System;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using Newtonsoft.Json.Linq;

namespace CSharpMath.FrontEnd {

  /// <summary>A wrapper class holding everything the core needs to have in order
  /// layout the LaTeX.</summary>
  public class TypesettingContext<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; }
    public IGlyphFinder<TFont, TGlyph> GlyphFinder { get; }
    
    public FontMathTable<TFont, TGlyph> MathTable { get; private set; }
    public Func<TFont, float, TFont> MathFontCloner { get; private set; }
    public IFontChanger FontChanger { get; private set; }

    public TypesettingContext(Func<TFont, float, TFont> mathFontCloner,
      IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider,
      IGlyphFinder<TFont, TGlyph> glyphFinder,
      IFontChanger fontChanger,
      FontMathTable<TFont, TGlyph> mathTable) {
      GlyphBoundsProvider = glyphBoundsProvider;
      MathFontCloner = mathFontCloner;
      GlyphFinder = glyphFinder;
      FontChanger = fontChanger;
      MathTable = mathTable;
    }

  }
}
