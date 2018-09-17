using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public struct Fonts : IMathFont<Glyph>, IEnumerable<Typeface> {
    static Fonts() {
      var reader = new OpenFontReader();
      var latinMathTypeface = reader.Read(new MemoryStream(Resources.LatinModernMath, false));
      latinMathTypeface.UpdateAllCffGlyphBounds();
      GlobalTypefaces = new Typefaces(latinMathTypeface);

      var amsBlackboardBoldTypeface = reader.Read(new MemoryStream(Resources.AMSCapitalBlackboardBold, false));
      amsBlackboardBoldTypeface.UpdateAllCffGlyphBounds();
      GlobalTypefaces.AddStart(amsBlackboardBoldTypeface);
    }

    public Fonts(IList<Typeface> localTypefaces, float pointSize) {
      PointSize = pointSize;
      var typefaces = localTypefaces.Concat(GlobalTypefaces);
      Typefaces = typefaces;
      MathTypeface = typefaces.First(t => t.HasMathTable());
    }
    public Fonts(Fonts cloneMe, float pointSize) {
      PointSize = pointSize;
      Typefaces = cloneMe.Typefaces;
      MathTypeface = cloneMe.MathTypeface;
    }

    public static Typefaces GlobalTypefaces { get; }

    public float PointSize { get; }
    public IEnumerable<Typeface> Typefaces { get; }
    public Typeface MathTypeface { get; }

    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts => MathTypeface.MathConsts;
    
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
