using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class MathFonts : MathFont<Glyph>, IEnumerable<Typeface> {
    static MathFonts() {
      var bytes = Resources.Otf;
      var reader = new OpenFontReader();
      var latinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      latinMathTypeface.UpdateAllCffGlyphBounds();
      GlobalTypefaces = new Typefaces(latinMathTypeface);
    }

    public static Typefaces GlobalTypefaces { get; }

    public MathFonts(IList<Typeface> localTypefaces, float pointSize) : this(localTypefaces.Concat(GlobalTypefaces), pointSize) { }
    public MathFonts(MathFonts cloneMe, float pointSize) : this(cloneMe.Typefaces, pointSize) { }

    private MathFonts(IEnumerable<Typeface> typefaces, float pointSize) : base(pointSize) {
      Typefaces = typefaces;
#warning Use HasMathTable
      MathTypeface = typefaces.First(t => t._mathTable != null);
      TypesettingContext = new TypesettingContext<MathFonts, Glyph>(
        //new FontMeasurer(),
        (fonts, size) => new MathFonts(fonts, size),
        new GlyphBoundsProvider(),
        //new GlyphNameProvider(someTypefaceSizeIrrelevant),
        new GlyphFinder(this),
        new UnicodeFontChanger(),
        //Resources.Json
        new MathTable()
      );
    }

    public IEnumerable<Typeface> Typefaces { get; }
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts => MathTypeface.MathConsts;
    public TypesettingContext<MathFonts, Glyph> TypesettingContext { get; }
    
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
