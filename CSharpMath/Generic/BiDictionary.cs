using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSharpMath
{
  //https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
  public class BiDictionary<TFirst, TSecond> : ICollection<KeyValuePair<TFirst, TSecond>> {
    readonly Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
    readonly Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

    public void Add(TFirst first, TSecond second) {
      if (firstToSecond.ContainsKey(first))
        throw new ArgumentException("Duplicate first");
      else if (secondToFirst.ContainsKey(second))
        throw new ArgumentException("Duplicate second");
      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
    }

    public Dictionary<TFirst, TSecond>.KeyCollection Firsts => firstToSecond.Keys;

    public Dictionary<TSecond, TFirst>.KeyCollection Seconds => secondToFirst.Keys;

    public int Count => firstToSecond.Count;

    public bool IsReadOnly => false;

    public TSecond this[TFirst first] => firstToSecond[first];

    public TFirst this[TSecond second] => secondToFirst[second];

    public bool TryGetByFirst(TFirst first, out TSecond second) =>
      firstToSecond.TryGetValue(first, out second);

    public bool TryGetBySecond(TSecond second, out TFirst first) =>
      secondToFirst.TryGetValue(second, out first);

    public Dictionary<TFirst, TSecond>.Enumerator GetEnumerator() =>
      firstToSecond.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => firstToSecond.GetEnumerator();
    IEnumerator<KeyValuePair<TFirst, TSecond>> IEnumerable<KeyValuePair<TFirst, TSecond>>.GetEnumerator() =>
      firstToSecond.GetEnumerator();

    public void Add(KeyValuePair<TFirst, TSecond> item) => Add(item.Key, item.Value);

    public void Clear() {
      firstToSecond.Clear();
      secondToFirst.Clear();
    }

    public bool Contains(TFirst first) => firstToSecond.ContainsKey(first);
    public bool Contains(TSecond second) => secondToFirst.ContainsKey(second);
    public bool Contains(KeyValuePair<TFirst, TSecond> pair) =>
      firstToSecond.TryGetValue(pair.Key, out var second) && EqualityComparer<TSecond>.Default.Equals(second, pair.Value);

    public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex) {
      foreach (var pair in firstToSecond)
        array[arrayIndex++] = pair;
    }

    public bool Remove(TFirst first, TSecond second) =>
      firstToSecond.Remove(first) &&
      secondToFirst.Remove(second);
    public bool Remove(KeyValuePair<TFirst, TSecond> pair) =>
      firstToSecond.Remove(pair.Key) &&
      secondToFirst.Remove(pair.Value);
  }

  public class MultiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>> {
    Dictionary<TFirst, IList<TSecond>> firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
    Dictionary<TSecond, IList<TFirst>> secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

    private static readonly ReadOnlyCollection<TFirst> EmptyFirstList =
      new ReadOnlyCollection<TFirst>(new TFirst[0]);
    private static readonly ReadOnlyCollection<TSecond> EmptySecondList =
      new ReadOnlyCollection<TSecond>(new TSecond[0]);

    public void Add(TFirst first, TSecond second) {
      if (!firstToSecond.TryGetValue(first, out IList<TSecond> seconds)) {
        seconds = new List<TSecond>();
        firstToSecond[first] = seconds;
      }
      if (!secondToFirst.TryGetValue(second, out IList<TFirst> firsts)) {
        firsts = new List<TFirst>();
        secondToFirst[second] = firsts;
      }
      seconds.Add(second);
      firsts.Add(first);
    }

    public ReadOnlyCollection<TSecond> this[TFirst first] {
      get {
        if (!firstToSecond.TryGetValue(first, out IList<TSecond> list)) {
          return EmptySecondList;
        }
        return new ReadOnlyCollection<TSecond>(list);
      }
    }

    public ReadOnlyCollection<TFirst> this[TSecond second] {
      get {
        if (!secondToFirst.TryGetValue(second, out IList<TFirst> list)) {
          return EmptyFirstList;
        }
        return new ReadOnlyCollection<TFirst>(list);
      }
    }

    public bool TryGetByFirst(TFirst first, out TSecond second) {
      if (firstToSecond.TryGetValue(first, out var list) && list.Count > 0) {
        second = list[0];
        return true;
      }
      second = default;
      return false;
    }

    public bool TryGetBySecond(TSecond second, out TFirst first) {
      if (secondToFirst.TryGetValue(second, out var list) && list.Count > 0) {
        first = list[0];
        return true;
      }
      first = default;
      return false;
    }

    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator() =>
      firstToSecond.SelectMany(p => p.Value.Select(i => new KeyValuePair<TFirst, TSecond>(p.Key, i))).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  }
}
