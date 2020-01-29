using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public struct Fonts : IFont<Glyph>, IEnumerable<Typeface> {
    static Fonts() {
      var reader = new OpenFontReader();
      Typeface LoadFont(string fileName) {
        var typeface = reader.Read(
          System.Reflection.Assembly.GetExecutingAssembly()
          .GetManifestResourceStream($"CSharpMath.Rendering.Font_Reference.{fileName}")
        );
        typeface.UpdateAllCffGlyphBounds();
        return typeface;
      }
      GlobalTypefaces = new Typefaces(LoadFont("latinmodern-math.otf"));
      GlobalTypefaces.AddStart(LoadFont("AMS-Capital-Blackboard-Bold.otf"));
      //GlobalTypefaces.AddEnd(LoadFont("cyrillic-modern-nmr5.otf")); // oof
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
