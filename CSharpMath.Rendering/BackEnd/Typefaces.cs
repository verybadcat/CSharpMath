using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;

namespace CSharpMath.Rendering.BackEnd {
  /// <summary>Typefaces stored in order of precedence.</summary>
  public class Typefaces : IEnumerable<Typeface> {
    internal Typefaces(Typeface _default) =>
      _typefaces = new Dictionary<sbyte, Typeface> { [0] = _default };
    private readonly IDictionary<sbyte, Typeface> _typefaces;
    /// <summary>Adds typeface at highest precedence.</summary>
    /// <param name="item"></param>
    public void AddOverride(Typeface item) =>
      _typefaces.Add(checked((sbyte)(_typefaces.Keys.Min() - 1)), item);
    /// <summary>Adds typeface at lowest precedence.</summary>
    public void AddSupplement(Typeface item) =>
      _typefaces.Add(checked((sbyte)(_typefaces.Keys.Max() + 1)), item);
    /// <summary>Gets typefaces, from highest to lowest precedence.</summary>
    public IEnumerator<Typeface> GetEnumerator() =>
      _typefaces.OrderBy(p => p.Key).Select(p => p.Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}