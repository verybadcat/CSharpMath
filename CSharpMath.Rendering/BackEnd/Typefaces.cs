using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace CSharpMath.Rendering.BackEnd {
  /// <summary>Typefaces stored in order of precedence.</summary>
  /// <remarks>Custom fonts need a valid OpenType MATH table, including variants or assemblies, for mathematical stretching.</remarks>
  public class Typefaces : IEnumerable<Typeface> {
    /// <param name="defaultTypeface">The initial default typeface.</param>
    public Typefaces(Typeface defaultTypeface) {
      EnsureGlyphBounds(defaultTypeface);
      _typefaces = new Dictionary<sbyte, Typeface> { [0] = defaultTypeface };
    }
    private readonly IDictionary<sbyte, Typeface> _typefaces;
    private readonly object _sync = new object();
    /// <summary>Adds typeface at highest precedence.</summary>
    /// <param name="item"></param>
    public void AddOverride(Typeface item) {
      EnsureGlyphBounds(item);
      lock (_sync)
        _typefaces.Add(checked((sbyte)(_typefaces.Keys.Min() - 1)), item);
    }
    /// <summary>Adds typeface at lowest precedence.</summary>
    public void AddSupplement(Typeface item) {
      EnsureGlyphBounds(item);
      lock (_sync)
        _typefaces.Add(checked((sbyte)(_typefaces.Keys.Max() + 1)), item);
    }
    internal static void EnsureGlyphBounds(Typeface typeface) {
      if (typeface.IsCffFont)
        typeface.UpdateAllCffGlyphBounds();
    }
    /// <summary>Gets typefaces, from highest to lowest precedence.</summary>
    public IEnumerator<Typeface> GetEnumerator() {
      lock (_sync)
        return _typefaces.OrderBy(p => p.Key).Select(p => p.Value).ToArray().AsEnumerable().GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
