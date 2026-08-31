using System;
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
      _localTypefaces = NormalizeLocals(localTypefaces);
      Typefaces = new DynamicTypefaces(_localTypefaces);
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
    private readonly IEnumerable<Typeface> _localTypefaces;
    internal IEnumerable<Typeface> LocalTypefacesSnapshot => _localTypefaces;
    public Typeface MathTypeface { get; }
    public Typography.OpenFont.MathGlyphs.MathConstants MathConsts { get; }
    public IEnumerator<Typeface> GetEnumerator() => Typefaces.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Typefaces.GetEnumerator();

    // Read globals at enumeration time so late AddSupplement/AddOverride calls
    // are visible to existing painters. Locals remain a defensive snapshot.
    sealed class DynamicTypefaces : IList<Typeface> {
      readonly IEnumerable<Typeface> locals;
      public DynamicTypefaces(IEnumerable<Typeface> locals) => this.locals = locals;
      IEnumerable<Typeface> Current => locals.Concat(GlobalTypefaces);
      public int Count => Current.Count();
      public bool IsReadOnly => true;
      public Typeface this[int index] { get => Current.ElementAt(index); set => throw new NotSupportedException(); }
      public IEnumerator<Typeface> GetEnumerator() => Current.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
      public bool Contains(Typeface item) => Current.Contains(item);
      public int IndexOf(Typeface item) => Current.ToList().IndexOf(item);
      public void CopyTo(Typeface[] array, int arrayIndex) => Current.ToList().CopyTo(array, arrayIndex);
      public void Add(Typeface item) => throw new NotSupportedException();
      public void Clear() => throw new NotSupportedException();
      public void Insert(int index, Typeface item) => throw new NotSupportedException();
      public bool Remove(Typeface item) => throw new NotSupportedException();
      public void RemoveAt(int index) => throw new NotSupportedException();
    }
    static IEnumerable<Typeface> NormalizeLocals(IEnumerable<Typeface> input) {
      if (input is Typeface[]) return input.ToArray();
      if (input is ICollection<Typeface> || input is IReadOnlyCollection<Typeface>) return input;
      return input.ToArray();
    }
  }
}
