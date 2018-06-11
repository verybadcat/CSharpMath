using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public class Typefaces : IList<Typeface>, IReadOnlyList<Typeface> {
    internal Typefaces(Typeface _default) => _typefaces = new List<Typeface>(System.Linq.Enumerable.Repeat(_default, 1));

    private IList<Typeface> _typefaces;

    public Typeface this[int index] { get => _typefaces[index]; set { if (index != 0) _typefaces[index] = value; } }

    public int Count => _typefaces.Count;

    public bool IsReadOnly => false;

    public void Add(Typeface item) => _typefaces.Add(item);

    public void Clear() { var item = _typefaces[0]; _typefaces.Clear(); _typefaces[0] = item; }

    public bool Contains(Typeface item) => _typefaces.Contains(item);

    public void CopyTo(Typeface[] array, int arrayIndex) => _typefaces.CopyTo(array, arrayIndex);

    public IEnumerator<Typeface> GetEnumerator() => _typefaces.GetEnumerator();

    public int IndexOf(Typeface item) => _typefaces.IndexOf(item);

    public void Insert(int index, Typeface item) { if (index != 0) _typefaces.Insert(index, item); }

    public bool Remove(Typeface item) { if (item == _typefaces[0]) return false; return _typefaces.Remove(item); }

    public void RemoveAt(int index) { if (index != 0) _typefaces.RemoveAt(index); }

    IEnumerator IEnumerable.GetEnumerator() => _typefaces.GetEnumerator();
  }
}