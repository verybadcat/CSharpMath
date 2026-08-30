using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.Atom;
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
    readonly IEnumerable<Typeface> localTypefaceSource;
    readonly Typeface[] localTypefaceSnapshot;
    readonly bool localTypefacesAreMutableCollection;
    internal IEnumerable<Typeface> LocalTypefaceSource => localTypefaceSource;
    // Collection mutations are observed between rendering operations; callers must not mutate
    // a collection concurrently with rendering.
    internal Typeface[] GetLocalTypefacesSnapshot() => localTypefacesAreMutableCollection
      ? localTypefaceSource.ToArray()
      : localTypefaceSnapshot;
    internal Typeface[] GetTypefacesSnapshot() => GetLocalTypefacesSnapshot().Concat(GlobalTypefaces).ToArray();
    internal Typeface[] GetTypefacesSnapshot(Typeface[] localTypefaces) => localTypefaces.Concat(GlobalTypefaces).ToArray();
    public Fonts(IEnumerable<Typeface> localTypefaces, float pointSize) {
      PointSize = pointSize;
      if (localTypefaces is Fonts fonts) {
        localTypefaceSource = fonts.LocalTypefaceSource;
        localTypefaceSnapshot = fonts.localTypefaceSnapshot;
        localTypefacesAreMutableCollection = fonts.localTypefacesAreMutableCollection;
      } else if (localTypefaces is ICollection<Typeface> || localTypefaces is IReadOnlyCollection<Typeface>) {
        localTypefaceSource = localTypefaces ?? Enumerable.Empty<Typeface>();
        localTypefaceSnapshot = null;
        localTypefacesAreMutableCollection = true;
      } else {
        localTypefaceSnapshot = (localTypefaces ?? Enumerable.Empty<Typeface>()).ToArray();
        localTypefaceSource = localTypefaceSnapshot;
        localTypefacesAreMutableCollection = false;
      }
      MathTypeface = GetTypefacesSnapshot().First(t => t.HasMathTable());
      MathConsts = MathTypeface.MathConsts ?? throw new Atom.InvalidCodePathException(nameof(MathTypeface) + " doesn't have " + nameof(MathConsts));
    }
    public static readonly Typefaces GlobalTypefaces = GetGlobalTypefaces();
    public float PointSize { get; }
    public IEnumerable<Typeface> Typefaces => GetTypefacesSnapshot();
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts { get; }
    public IEnumerator<Typeface> GetEnumerator() => GetTypefacesSnapshot().AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();
  }
}
