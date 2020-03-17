using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;

namespace CSharpMath.Rendering.BackEnd {
  public readonly struct Fonts : Display.FrontEnd.IFont<Glyph>, IEnumerable<Typeface> {
    static Fonts() {
      var reader = new OpenFontReader();
      Typeface LoadFont(string fileName) {
        var typeface = reader.Read(
          System.Reflection.Assembly.GetExecutingAssembly()
          .GetManifestResourceStream($"CSharpMath.Rendering.Reference_Fonts.{fileName}")
        );
        typeface.UpdateAllCffGlyphBounds();
        return typeface;
      }
      GlobalTypefaces = new Typefaces(LoadFont("latinmodern-math.otf"));
      GlobalTypefaces.AddOverride(LoadFont("AMS-Capital-Blackboard-Bold.otf"));
      GlobalTypefaces.AddSupplement(LoadFont("cyrillic-modern-nmr10.otf"));
    }
    public Fonts(IList<Typeface> localTypefaces, float pointSize) {
      PointSize = pointSize;
      Typefaces = localTypefaces.Concat(GlobalTypefaces);
      MathTypeface = Typefaces.First(t => t.HasMathTable());
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
