using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;

namespace CSharpMath.Rendering.BackEnd {
  public class Typefaces : IEnumerable<Typeface> {
    internal Typefaces(Typeface _default) =>
      _typefaces = new Dictionary<sbyte, Typeface> { [0] = _default };
    private readonly IDictionary<sbyte, Typeface> _typefaces;
    public void AddOverride(Typeface item) =>
      _typefaces.Add(checked((sbyte)(_typefaces.Keys.Min() - 1)), item);
    public void AddSupplement(Typeface item) =>
      _typefaces.Add(checked((sbyte)(_typefaces.Keys.Max() + 1)), item);
    public IEnumerator<Typeface> GetEnumerator() =>
      _typefaces.OrderBy(p => p.Key).Select(p => p.Value).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}