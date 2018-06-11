using System.Collections;
using System.Collections.Generic;
using CSharpMath.Display;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class MathFonts : MathFont<Glyph>, IReadOnlyCollection<Typeface>
  {
    private Typefaces GlobalTypefaces => FontManager.GlobalTypefaces;

    private IList<Typeface> LocalTypefaces { get; }
    
    public MathFonts(IList<Typeface> localTypefaces, float pointSize) : base(pointSize) => LocalTypefaces = localTypefaces ?? System.Array.Empty<Typeface>();

    public MathFonts(MathFonts cloneMe, float pointSize) : base(pointSize) => LocalTypefaces = cloneMe.LocalTypefaces;

    public int Count => GlobalTypefaces.Count + LocalTypefaces.Count;

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    IEnumerator<Typeface> IEnumerable<Typeface>.GetEnumerator() => new Enumerator(this);

    public struct Enumerator : IEnumerator<Typeface> {
      bool _enumerated;

      bool _enumeratingGlobal;

      MathFonts _fonts;

      internal Enumerator(MathFonts fonts) : this() {
        _fonts = fonts;
      }

      public Typeface Current { get; private set; }

      object IEnumerator.Current => Current;

      public void Dispose() { }

      //Last of local typefaces -> first of local typefaces -> last of global typefaces -> first of global typefaces
      public bool MoveNext() {
        if (_enumerated) throw new System.InvalidOperationException("The end of enumeration has already been reached.");
        if (_enumeratingGlobal) {
          //global typefaces are never empty (see FontManager)
          if (Current == null) {
            Current = _fonts.GlobalTypefaces[_fonts.GlobalTypefaces.Count - 1]; //start from end of global typefaces
            return true;
          }
          var index = _fonts.GlobalTypefaces.IndexOf(Current);
          if (index == 0) { _enumerated = true; return false; } //reached the end
          Current = _fonts.GlobalTypefaces[index - 1]; //get the next global typeface
          return true;
        } else {
          var count = _fonts.LocalTypefaces.Count;
          if (count == 0) {
            _enumeratingGlobal = true;
            return MoveNext(); //local typefaces are empty, enumerate global typefaces
          }
          if (Current == null) {
            Current = _fonts.LocalTypefaces[count - 1];
            return true; //start from end of local typefaces
          }
          var index = _fonts.LocalTypefaces.IndexOf(Current);
          if (index == 0) {
            Current = null;
            _enumeratingGlobal = true;
            return MoveNext(); //reached the end of local typefaces, enumerate global typefaces
          }
          Current = _fonts.LocalTypefaces[index - 1]; //get the next local typeface
          return true;
        }
      }

      public void Reset() {
        _enumerated = false;
        _enumeratingGlobal = false;
        Current = null;
      }
    }
  }
}
