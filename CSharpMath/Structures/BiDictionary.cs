using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSharpMath.Structures {
  public class AliasDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> {
    readonly Dictionary<TKey, TValue> k2v = new Dictionary<TKey, TValue>();
    readonly Dictionary<TValue, TKey> v2k = new Dictionary<TValue, TKey>();
    public TValue this[TKey key] { get => k2v[key]; set { k2v[key] = value; v2k[value] = key; } }
    public TKey this[TValue val] { get => v2k[val]; set { v2k[val] = value; k2v[value] = val; } }
    public int Count => k2v.Count;
    public bool IsReadOnly => false;
    public Dictionary<TKey, TValue>.KeyCollection Keys => k2v.Keys;
    public Dictionary<TValue, TKey>.KeyCollection Values => v2k.Keys;
    #region AliasDictionary<K, V>.Add
    public void Add(ReadOnlySpan<TKey> keys, TValue value) {
      if (!v2k.ContainsKey(value) && !keys.IsEmpty) v2k.Add(value, keys[0]);
      foreach (var key in keys)
        k2v.Add(key, value);
    }
    //Array renting may result in larger arrays than normal -> the unused slots are nulls.
    //Therefore, slicing prevents nulls from propagating through.
    public void Add(TKey mainKey, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(1);
      array[0] = mainKey;
      Add(new ReadOnlySpan<TKey>(array, 0, 1), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(2);
      array[0] = mainKey;
      array[1] = aliasKey;
      Add(new ReadOnlySpan<TKey>(array, 0, 2), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(3);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      Add(new ReadOnlySpan<TKey>(array, 0, 3), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(4);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      Add(new ReadOnlySpan<TKey>(array, 0, 4), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TKey aliasKey4, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(5);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      Add(new ReadOnlySpan<TKey>(array, 0, 5), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TKey aliasKey4,
      TKey aliasKey5, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(6);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      Add(new ReadOnlySpan<TKey>(array, 0, 6), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TKey aliasKey4,
      TKey aliasKey5, TKey aliasKey6, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(7);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      Add(new ReadOnlySpan<TKey>(array, 0, 7), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TKey aliasKey4,
      TKey aliasKey5, TKey aliasKey6, TKey aliasKey7, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(8);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      array[7] = aliasKey7;
      Add(new ReadOnlySpan<TKey>(array, 0, 8), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    public void Add(TKey mainKey, TKey aliasKey1, TKey aliasKey2, TKey aliasKey3, TKey aliasKey4,
      TKey aliasKey5, TKey aliasKey6, TKey aliasKey7, TKey aliasKey8, TValue value) {
      var array = ArrayPool<TKey>.Shared.Rent(9);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      array[5] = aliasKey5;
      array[6] = aliasKey6;
      array[7] = aliasKey7;
      array[8] = aliasKey8;
      Add(new ReadOnlySpan<TKey>(array, 0, 9), value);
      ArrayPool<TKey>.Shared.Return(array);
    }
    #endregion AliasDictionary<K, V>.Add
    public void Clear() {
      k2v.Clear();
      v2k.Clear();
    }
    public bool Contains(KeyValuePair<TKey, TValue> pair) =>
      k2v.TryGetValue(pair.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, pair.Value);
    public bool ContainsKey(TKey key) => k2v.ContainsKey(key);
    public bool ContainsValue(TValue value) => v2k.ContainsKey(value);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
      ((ICollection<KeyValuePair<TKey, TValue>>)k2v).CopyTo(array, arrayIndex);
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => k2v.GetEnumerator();
    public bool Remove(TValue value) {
      if (!v2k.Remove(value)) return false;
      foreach (var pair in k2v.Where(p => EqualityComparer<TValue>.Default.Equals(p.Value, value)))
        k2v.Remove(pair.Key);
      return true;
    }
    public bool Remove(TKey key) => Remove(key, k2v[key]);
    public bool Remove(KeyValuePair<TKey, TValue> pair) => Remove(pair.Key, pair.Value);
    public bool Remove(TKey key, TValue value) {
      var valueMatches = k2v.Where(p => EqualityComparer<TValue>.Default.Equals(p.Value, value)).ToList();
      switch (valueMatches.Count) {
        case 0:
          return false;
        case 1:
          if (!EqualityComparer<TKey>.Default.Equals(valueMatches[0].Key, key)) return false;
          k2v.Remove(key);
          v2k.Remove(value);
          return true;
        case var _:
          if (!valueMatches.Any(p => EqualityComparer<TKey>.Default.Equals(p.Key, key))) return false;
          k2v.Remove(key);
          if (EqualityComparer<TKey>.Default.Equals(v2k[value], key))
            v2k[value] = valueMatches.First(p => !EqualityComparer<TKey>.Default.Equals(p.Key, key)).Key;
          return true;
      }
    }
    public bool TryGetKey(TValue value, out TKey key) => v2k.TryGetValue(value, out key);
    public bool TryGetValue(TKey key, out TValue value) => k2v.TryGetValue(key, out value);
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) =>
      Add(item.Key, item.Value);
    IEnumerator IEnumerable.GetEnumerator() => k2v.GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
      k2v.GetEnumerator();
    ICollection<TKey> IDictionary<TKey, TValue>.Keys => k2v.Keys;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => k2v.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => v2k.Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => v2k.Keys;
  }

  //https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
  public class BiDictionary<TFirst, TSecond>
    : IDictionary<TFirst, TSecond>, IReadOnlyDictionary<TFirst, TSecond> {
    readonly Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
    readonly Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();
    public TSecond this[TFirst first] {
      get => firstToSecond[first];
      set => AddOrReplace(first, value);
    }
    public TFirst this[TSecond second] {
      get => secondToFirst[second];
      set => AddOrReplace(value, second);
    }
    public int Count => firstToSecond.Count;
    public Dictionary<TFirst, TSecond>.KeyCollection Firsts => firstToSecond.Keys;
    public bool IsReadOnly => false;
    public Dictionary<TSecond, TFirst>.KeyCollection Seconds => secondToFirst.Keys;
    public void Add(TFirst first, TSecond second) {
      // Call the Add() that will throw first
      if (firstToSecond.ContainsKey(first))
        firstToSecond.Add(first, second);
      else if (secondToFirst.ContainsKey(second))
        secondToFirst.Add(second, first);

      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
    }
    public void Add(KeyValuePair<TFirst, TSecond> item) => Add(item.Key, item.Value);
    public void AddOrReplace(TFirst first, TSecond second) {
      if (firstToSecond.ContainsKey(first))
        RemoveByFirst(first);
      if (secondToFirst.ContainsKey(second))
        RemoveBySecond(second);
      firstToSecond.Add(first, second);
      secondToFirst.Add(second, first);
    }
    public void AddOrReplace(KeyValuePair<TFirst, TSecond> item) => AddOrReplace(item.Key, item.Value);
    public void Clear() {
      firstToSecond.Clear();
      secondToFirst.Clear();
    }
    public bool ContainsByFirst(TFirst first) => firstToSecond.ContainsKey(first);
    public bool ContainsBySecond(TSecond second) => secondToFirst.ContainsKey(second);
    public bool Contains(KeyValuePair<TFirst, TSecond> pair) =>
      firstToSecond.TryGetValue(pair.Key, out var second)
      && EqualityComparer<TSecond>.Default.Equals(second, pair.Value);
    public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex) {
      if (array is null) throw new ArgumentNullException(nameof(array));
      foreach (var pair in firstToSecond)
        array[arrayIndex++] = pair;
    }
    public Dictionary<TFirst, TSecond>.Enumerator GetEnumerator() =>
      firstToSecond.GetEnumerator();
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
    public bool TryGetByFirst(TFirst first, out TSecond second) =>
      firstToSecond.TryGetValue(first, out second);
    public bool TryGetBySecond(TSecond second, out TFirst first) =>
      secondToFirst.TryGetValue(second, out first);
#pragma warning disable CA1033 // Interface methods should be callable by child types
    bool IDictionary<TFirst, TSecond>.ContainsKey(TFirst first) => firstToSecond.ContainsKey(first);
    bool IReadOnlyDictionary<TFirst, TSecond>.ContainsKey(TFirst first) => firstToSecond.ContainsKey(first);
    IEnumerator IEnumerable.GetEnumerator() => firstToSecond.GetEnumerator();
    IEnumerator<KeyValuePair<TFirst, TSecond>> IEnumerable<KeyValuePair<TFirst, TSecond>>.GetEnumerator() =>
      firstToSecond.GetEnumerator();
    ICollection<TFirst> IDictionary<TFirst, TSecond>.Keys => Firsts;
    IEnumerable<TFirst> IReadOnlyDictionary<TFirst, TSecond>.Keys => Firsts;
    bool IDictionary<TFirst, TSecond>.Remove(TFirst first) => Remove(first, firstToSecond[first]);
    bool IDictionary<TFirst, TSecond>.TryGetValue(TFirst first, out TSecond second) =>
      firstToSecond.TryGetValue(first, out second);
    bool IReadOnlyDictionary<TFirst, TSecond>.TryGetValue(TFirst first, out TSecond second) =>
      firstToSecond.TryGetValue(first, out second);
    ICollection<TSecond> IDictionary<TFirst, TSecond>.Values => Seconds;
    IEnumerable<TSecond> IReadOnlyDictionary<TFirst, TSecond>.Values => Seconds;
#pragma warning restore CA1033 // Interface methods should be callable by child types
  }
#pragma warning disable CA1710 // Identifiers should have correct suffix
  public class MultiDictionary<TFirst, TSecond> : IEnumerable<KeyValuePair<TFirst, TSecond>> {
#pragma warning restore CA1710 // Identifiers should have correct suffix
    readonly Dictionary<TFirst, IList<TSecond>> firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
    readonly Dictionary<TSecond, IList<TFirst>> secondToFirst = new Dictionary<TSecond, IList<TFirst>>();
    private static readonly ReadOnlyCollection<TFirst> EmptyFirstList =
      new ReadOnlyCollection<TFirst>(Array.Empty<TFirst>());
    private static readonly ReadOnlyCollection<TSecond> EmptySecondList =
      new ReadOnlyCollection<TSecond>(Array.Empty<TSecond>());
    public ReadOnlyCollection<TSecond> this[TFirst first] =>
      firstToSecond.TryGetValue(first, out var list) ? new ReadOnlyCollection<TSecond>(list) : EmptySecondList;
    public ReadOnlyCollection<TFirst> this[TSecond second] =>
      secondToFirst.TryGetValue(second, out var list) ? new ReadOnlyCollection<TFirst>(list) : EmptyFirstList;
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
    public bool TryGetByFirst(TFirst first, out TSecond second) {
      if (firstToSecond.TryGetValue(first, out var list) && list.Count > 0) {
        second = list[0];
        return true;
      }
      second = default!;
      return false;
    }
    public bool TryGetBySecond(TSecond second, out TFirst first) {
      if (secondToFirst.TryGetValue(second, out var list) && list.Count > 0) {
        first = list[0];
        return true;
      }
      first = default!;
      return false;
    }
    public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator() =>
      firstToSecond.SelectMany(p => p.Value.Select(v => new KeyValuePair<TFirst, TSecond>(p.Key, v)))
        .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}