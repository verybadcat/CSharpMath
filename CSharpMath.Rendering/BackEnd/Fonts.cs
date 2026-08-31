using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace CSharpMath.Rendering.BackEnd {
  public class Fonts : Display.FrontEnd.IFont<Glyph>, IEnumerable<Typeface> {
    static Typefaces GetGlobalTypefaces() {
      var reader = new OpenFontReader();
      Typeface LoadFont(string fileName) {
        var typeface = reader.Read(
          System.Reflection.Assembly.GetExecutingAssembly()
          .GetManifestResourceStream($"CSharpMath.Rendering.Reference_Fonts.{fileName}")
        );
        if (typeface == null) throw new Atom.InvalidCodePathException("Invalid predefined font!");
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
      _localTypefaces = localTypefaces.ToArray();
      Typefaces = new System.Collections.ObjectModel.ReadOnlyCollection<Typeface>(
        _localTypefaces.Concat(GlobalTypefaces).ToArray());
      LocalTypefacesSnapshot = new System.Collections.ObjectModel.ReadOnlyCollection<Typeface>(_localTypefaces);
      MathTypeface = Typefaces.First(t => t.HasMathTable());
      MathConsts = MathTypeface.MathConsts ?? throw new Atom.InvalidCodePathException(nameof(MathTypeface) + " doesn't have " + nameof(MathConsts));
    }
    // The public constructor clones this private local-only snapshot. It must
    // not receive Typefaces, which also contains globals and would retain every
    // preceding Fonts instance through a growing concatenation chain.
    internal static Fonts Resize(Fonts source, float pointSize) =>
      new Fonts(source._localTypefaces, pointSize);
    public static readonly Typefaces GlobalTypefaces = GetGlobalTypefaces();
    public float PointSize { get; }
    public IEnumerable<Typeface> Typefaces { get; }
    private readonly Typeface[] _localTypefaces;
    internal IReadOnlyList<Typeface> LocalTypefacesSnapshot { get; }
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts { get; }
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
