using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class Fonts : MathFont<Glyph>, IEnumerable<Typeface> {
    static Fonts() {
      var bytes = Resources.Otf;
      var reader = new OpenFontReader();
      var latinMathTypeface = reader.Read(new MemoryStream(bytes, false));
      latinMathTypeface.UpdateAllCffGlyphBounds();
      GlobalTypefaces = new Typefaces(latinMathTypeface);
    }

    public static Typefaces GlobalTypefaces { get; }

    public Fonts(IList<Typeface> localTypefaces, float pointSize) : base(pointSize) {
      var typefaces = localTypefaces.Concat(GlobalTypefaces);
      Typefaces = typefaces;
      MathTypeface = typefaces.First(t => t.HasMathTable());
    }
    public Fonts(Fonts cloneMe, float pointSize) : base(pointSize) {
      Typefaces = cloneMe.Typefaces;
      MathTypeface = cloneMe.MathTypeface;
    }

    public IEnumerable<Typeface> Typefaces { get; }
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts => MathTypeface.MathConsts;
    
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
