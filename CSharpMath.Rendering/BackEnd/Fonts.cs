using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;

namespace CSharpMath.Rendering.BackEnd {
  public class Fonts : Display.FrontEnd.IFont<Glyph>, IEnumerable<Typeface> {
    static Typefaces GetGlobalTypefaces() {
      var reader = new OpenFontReader();
      Typeface LoadFont(string fileName) {
        var typeface = reader.Read(
          System.Reflection.Assembly.GetExecutingAssembly()
          .GetManifestResourceStream($"CSharpMath.Rendering.Reference_Fonts.{fileName}")
        );
        if (typeface == null) throw new Structures.InvalidCodePathException("Invalid predefined font!");
        typeface.UpdateAllCffGlyphBounds();
        return typeface;
      }
      var globalTypefaces = new Typefaces(LoadFont("latinmodern-math.otf"));
      globalTypefaces.AddOverride(LoadFont("AMS-Capital-Blackboard-Bold.otf"));
      globalTypefaces.AddSupplement(LoadFont("cyrillic-modern-nmr10.otf"));
      return globalTypefaces;
    }
    public Fonts(IEnumerable<Typeface> localTypefaces, float pointSize) {
      PointSize = pointSize;
      Typefaces = localTypefaces.Concat(GlobalTypefaces);
      MathTypeface = Typefaces.First(t => t.HasMathTable());
      MathConsts = MathTypeface.MathConsts ?? throw new Structures.InvalidCodePathException(nameof(MathTypeface) + " doesn't have " + nameof(MathConsts));
    }
    public static readonly Typefaces GlobalTypefaces = GetGlobalTypefaces();
    public float PointSize { get; }
    public IEnumerable<Typeface> Typefaces { get; }
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts { get; }
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
