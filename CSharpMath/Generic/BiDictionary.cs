using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSharpMath
{
  public class AliasDictionary<K, V> : ICollection<KeyValuePair<K, V>> {
    public AliasDictionary() {
      aliases = new Dictionary<K, V>();
      main = new BiDictionary<K, V>();
    }
    public AliasDictionary(int capacity) {
      aliases = new Dictionary<K, V>(capacity);
      main = new BiDictionary<K, V>(capacity);
    }

    readonly Dictionary<K, V> aliases;
    readonly BiDictionary<K, V> main;

    #region AliasDictionary<K, V>.Add
    public void Add(IEnumerable<K> keys, V value) {
      if (main.Contains(value))
        foreach (var key in keys)
          aliases.Add(key, value);
      else if(keys.Any()) {
        main.Add(keys.First(), value);
        foreach (var key in keys.Skip(1))
          aliases.Add(key, value);
      }
    }
    public void Add(K mainKey, V value) {
      var array = ArrayPool<K>.Shared.Rent(1);
      array[0] = mainKey;
      Add(array, value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey, V value) {
      var array = ArrayPool<K>.Shared.Rent(2);
      array[0] = mainKey;
      array[1] = aliasKey;
      Add(array, value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, V value) {
      var array = ArrayPool<K>.Shared.Rent(3);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      Add(array, value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, V value) {
      var array = ArrayPool<K>.Shared.Rent(4);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      Add(array, value);
      ArrayPool<K>.Shared.Return(array);
    }
    public void Add(K mainKey, K aliasKey1, K aliasKey2, K aliasKey3, K aliasKey4, V value) {
      var array = ArrayPool<K>.Shared.Rent(5);
      array[0] = mainKey;
      array[1] = aliasKey1;
      array[2] = aliasKey2;
      array[3] = aliasKey3;
      array[4] = aliasKey4;
      Add(array, value);
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
      Add(array, value);
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
      Add(array, value);
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
      Add(array, value);
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
      Add(array, value);
      ArrayPool<K>.Shared.Return(array);
    }
    #endregion AliasDictionary<K, V>.Add

    public IEnumerable<K> Keys => main.Firsts.Concat(aliases.Keys);
    public IEnumerable<V> Values => main.Seconds;

    public int Count => aliases.Keys.Count + main.Firsts.Count;

    public bool IsReadOnly => false;

    public V this[K key] => main.TryGetByFirst(key, out var second) ? second : aliases[key];

    public K this[V value] => main[value];

    public bool TryGetValue(K key, out V value) =>
      main.TryGetByFirst(key, out value) || aliases.TryGetValue(key, out value);

    public bool TryGetKey(V value, out K key) =>
      main.TryGetBySecond(value, out key);

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() =>
      main.Concat(aliases).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => main.Concat(aliases).GetEnumerator();
    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() =>
      main.Concat(aliases).GetEnumerator();

    void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

    public void Clear() {
      main.Clear();
      aliases.Clear();
    }

    public bool ContainsKey(K key) => main.Contains(key) || aliases.ContainsKey(key);
    public bool ContainsValue(V value) => main.Contains(value);
    public bool Contains(KeyValuePair<K, V> pair) =>
      (main.TryGetByFirst(pair.Key, out var second) || aliases.TryGetValue(pair.Key, out second)) && EqualityComparer<V>.Default.Equals(second, pair.Value);

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
      foreach (var pair in main.Concat(aliases))
        array[arrayIndex++] = pair;
    }

    public bool Remove(K first, V second) =>
      main.Remove(first, second) || aliases.Remove(first);
    public bool Remove(KeyValuePair<K, V> pair) =>
      main.Remove(pair) || aliases.Remove(pair.Key);
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
    public MultiDictionary() : base() {
      firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
      secondToFirst = new Dictionary<TSecond, IList<TFirst>>();
    }
    public MultiDictionary(int capacity) {
      firstToSecond = new Dictionary<TFirst, IList<TSecond>>(capacity);
      secondToFirst = new Dictionary<TSecond, IList<TFirst>>(capacity);
    }
    Dictionary<TFirst, IList<TSecond>> firstToSecond;
    Dictionary<TSecond, IList<TFirst>> secondToFirst;

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
