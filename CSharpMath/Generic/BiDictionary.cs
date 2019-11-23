using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSharpMath
{
  public class AliasDictionary<K, V> : ICollection<KeyValuePair<K, V>>, IDictionary<K, V> {
    public AliasDictionary() {
      k2v = new Dictionary<K, V>();
      v2k = new Dictionary<V, K>();
    }

    readonly Dictionary<K, V> k2v;
    readonly Dictionary<V, K> v2k;

    #region AliasDictionary<K, V>.Add
    public void Add(ReadOnlySpan<K> keys, V value) {
      if (!v2k.ContainsKey(value) && !keys.IsEmpty) v2k.Add(value, keys[0]);
      foreach (var key in keys)
        k2v.Add(key, value);
    }
    //Array renting may result in larger arrays than normal -> the unused slots are nulls.
    //Therefore, slicing prevents nulls from propagating through.
    public void Add(K mainKey, V value) {
      var array = ArrayPool<K>.Shared.Rent(1);
      array[0] = mainKey;
      Add(new ReadOnlySpan<K>(array, 0, 1), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey, V value) {
      var array = ArrayPool<K>.Shared.Rent(2);
      array[0] = mainKey;
      array[1] = aliasKey;
      Add(new ReadOnlySpan<K>(array, 0, 2), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, V value) {
      var array = ArrayPool<K>.Shared.Rent(3);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      Add(new ReadOnlySpan<K>(array, 0, 3), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, V value) {
      var array = ArrayPool<K>.Shared.Rent(4);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      Add(new ReadOnlySpan<K>(array, 0, 4), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, V value) {
      var array = ArrayPool<K>.Shared.Rent(5);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      Add(new ReadOnlySpan<K>(array, 0, 5), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, K aliasKey5, V value) {
      var array = ArrayPool<K>.Shared.Rent(6);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      Add(new ReadOnlySpan<K>(array, 0, 6), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, K aliasKey5, K aliasKey6, V value) {
      var array = ArrayPool<K>.Shared.Rent(7);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      Add(new ReadOnlySpan<K>(array, 0, 7), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, K aliasKey5, K aliasKey6, K aliasKey7, V value) {
      var array = ArrayPool<K>.Shared.Rent(8);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      array[7] = aliasKey7;
      Add(new ReadOnlySpan<K>(array, 0, 8), value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, K aliasKey5, K aliasKey6, K aliasKey7, K aliasKey8, V value) {
      var array = ArrayPool<K>.Shared.Rent(9);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      array[7] = aliasKey7;
      array[8] = aliasKey8;
      Add(new ReadOnlySpan<K>(array, 0, 9), value);
      ArrayPool<K>.Shared.Return(array);
    }
    #endregion AliasDictionary<K, V>.Add

    public Dictionary<K, V>.KeyCollection Keys => k2v.Keys;
    public Dictionary<V, K>.KeyCollection Values => v2k.Keys;
    ICollection<K> IDictionary<K, V>.Keys => k2v.Keys;
    ICollection<V> IDictionary<K, V>.Values => v2k.Keys;

    public int Count => k2v.Count;

    public bool IsReadOnly => false;

    public V this[K key] { get => k2v[key]; set { k2v[key] = value; v2k[value] = key; } }

    public K this[V Value] { get => v2k[Value]; set { v2k[Value] = value; k2v[value] = Value; } }

    public bool TryGetValue(K key, out V value) => k2v.TryGetValue(key, out value);

    public bool TryGetKey(V value, out K key) => v2k.TryGetValue(value, out key);

    public Dictionary<K, V>.Enumerator GetEnumerator() => k2v.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => k2v.GetEnumerator();
    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => k2v.GetEnumerator();

    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

    public void Clear() {
      k2v.Clear();
      v2k.Clear();
    }

    public bool ContainsKey(K key) => k2v.ContainsKey(key);
    public bool ContainsValue(V value) => v2k.ContainsKey(value);
    public bool Contains(KeyValuePair<K, V> pair) =>
      k2v.TryGetValue(pair.Key, out var value) && EqualityComparer<V>.Default.Equals(value, pair.Value);

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) =>
      ((ICollection<KeyValuePair<K, V>>)k2v).CopyTo(array, arrayIndex);

    public bool Remove(V value) {
      if (!v2k.Remove(value)) return false;
      foreach (var pair in k2v.Where(p => EqualityComparer<V>.Default.Equals(p.Value, value)))
        k2v.Remove(pair.Key);
      return true;
    }
    public bool Remove(K key) => Remove(key, k2v[key]);
    public bool Remove(KeyValuePair<K, V> pair) => Remove(pair.Key, pair.Value);
    public bool Remove(K key, V value) {
      var valueMatches = k2v.Where(p => EqualityComparer<V>.Default.Equals(p.Value, value)).ToList();
      switch (valueMatches.Count) {
        case 0:
          return false;
        case 1:
          if (!EqualityComparer<K>.Default.Equals(valueMatches[0].Key, key)) return false;
          k2v.Remove(key);
          v2k.Remove(value);
          return true;
        case var _:
          if (!valueMatches.Any(p => EqualityComparer<K>.Default.Equals(p.Key, key))) return false;
          k2v.Remove(key);
          if (EqualityComparer<K>.Default.Equals(v2k[value], key))
            v2k[value] = valueMatches.First(p => !EqualityComparer<K>.Default.Equals(p.Key, key)).Key;
          return true;
      }
    }
  }

  //https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
  public class BiDictionary<TFirst, TSecond> : ICollection<KeyValuePair<TFirst, TSecond>> {
    public BiDictionary() : base() {
      firstToSecond = new Dictionary<TFirst, TSecond>();
      secondToFirst = new Dictionary<TSecond, TFirst>();
    }
    public BiDictionary(int capacity) {
      firstToSecond = new Dictionary<TFirst, TSecond>(capacity);
      secondToFirst = new Dictionary<TSecond, TFirst>(capacity);
    }

    readonly Dictionary<TFirst, TSecond> firstToSecond;
    readonly Dictionary<TSecond, TFirst> secondToFirst;

    public void Add(TFirst first, TSecond second) {
      if (firstToSecond.ContainsKey(first))
        throw new ArgumentException("Duplicate first", nameof(first));
      else if (secondToFirst.ContainsKey(second))
        throw new ArgumentException("Duplicate second", nameof(second));
      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
    }

    public void AddOrReplace(TFirst first, TSecond second) {
      if (firstToSecond.ContainsKey(first))
        RemoveByFirst(first);
      if (secondToFirst.ContainsKey(second))
        RemoveBySecond(second);
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
    public void AddOrReplace(KeyValuePair<TFirst, TSecond> item) => AddOrReplace(item.Key, item.Value);
    public void Clear() {
      firstToSecond.Clear();
      secondToFirst.Clear();
    }

    public bool ContainsByFirst(TFirst first) => firstToSecond.ContainsKey(first);
    public bool ContainsBySecond(TSecond second) => secondToFirst.ContainsKey(second);
    public bool Contains(KeyValuePair<TFirst, TSecond> pair) =>
      firstToSecond.TryGetValue(pair.Key, out var second) && EqualityComparer<TSecond>.Default.Equals(second, pair.Value);

    public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex) {
      foreach (var pair in firstToSecond)
        array[arrayIndex++] = pair;
    }

    public bool Remove(TFirst first, TSecond second) {
      if (TryGetByFirst(first, out var svalue) && TryGetBySecond(second, out var fvalue)) {

        firstToSecond.Remove(first);
        firstToSecond.Remove(fvalue);

        secondToFirst.Remove(second);
        secondToFirst.Remove(svalue);
        return true;
      }
      return false;
    }

    public bool Remove(KeyValuePair<TFirst, TSecond> pair) => Remove(pair.Key, pair.Value);
    public bool RemoveByFirst(TFirst first) => Remove(first, firstToSecond[first]);
    public bool RemoveBySecond(TSecond second) => Remove(secondToFirst[second], second);
  }

  public class MultiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>> {
    public MultiDictionary() : base() {
      firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
      secondToFirst = new Dictionary<TSecond, IList<TFirst>>();
    }
    public MultiDictionary(int capacity) {
      firstToSecond = new Dictionary<TFirst, IList<TSecond>>(capacity);
      secondToFirst = new Dictionary<TSecond, IList<TFirst>>(capacity);
    }

    readonly Dictionary<TFirst, IList<TSecond>> firstToSecond;
    readonly Dictionary<TSecond, IList<TFirst>> secondToFirst;

    private static readonly ReadOnlyCollection<TFirst> EmptyFirstList =
      new ReadOnlyCollection<TFirst>(new TFirst[0]);
    private static readonly ReadOnlyCollection<TSecond> EmptySecondList =
      new ReadOnlyCollection<TSecond>(new TSecond[0]);

    public void Add(TFirst first, TSecond second) {
      if (!firstToSecond.TryGetValue(first, out var seconds)) {
        seconds = new List<TSecond>();
        firstToSecond[first] = seconds;
      }
      if (!secondToFirst.TryGetValue(second, out var firsts)) {
        firsts = new List<TFirst>();
        secondToFirst[second] = firsts;
      }
      seconds.Add(second);
      firsts.Add(first);
    }

    public ReadOnlyCollection<TSecond> this[TFirst first] =>
      firstToSecond.TryGetValue(first, out var list) ? new ReadOnlyCollection<TSecond>(list) : EmptySecondList;

    public ReadOnlyCollection<TFirst> this[TSecond second] =>
      secondToFirst.TryGetValue(second, out var list) ? new ReadOnlyCollection<TFirst>(list) : EmptyFirstList;

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
