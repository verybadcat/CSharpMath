using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CSharpMath.Structures {
  /// <summary>
  /// Funnels collection initializers to <see cref="Added"/>.
  /// Implements <see cref="IEnumerable"/> but throws upon enumeration because it is there only to enable collection initializers.
  /// </summary>
  /// <example>
  /// <code>new ProxyAdder&lt;int, string&gt;((key, value) => Console.WriteLine(value))
  /// { { 1, 2, 3, "1 to 3" }, { Enumerable.Range(7, 10), i => i.ToString() } }</code>
  /// </example>
  [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = NotACollection)]
  [SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = NotACollection)]
  public class ProxyAdder<TKey, TValue> : IEnumerable {
    const string NotACollection = "This is not a collection. It implements IEnumerable just to support collection initializers.";
    [Obsolete(NotACollection, true)]
    [SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = NotACollection)]
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException(NotACollection);
    public ProxyAdder(Action<TKey, TValue>? added = null) => Added += added;
    public event Action<TKey, TValue>? Added;
    public void Add(TKey key1, TValue value) => Added?.Invoke(key1, value);
    public void Add(TKey key1, TKey key2, TValue value) {
      Add(key1, value); Add(key2, value);
    }
    public void Add(TKey key1, TKey key2, TKey key3, TValue value) {
      Add(key1, value); Add(key2, value); Add(key3, value);
    }
    public void Add<TCollection>(TCollection keys, TValue value) where TCollection : IEnumerable<TKey> {
      foreach (var key in keys) Add(key, value);
    }
    public void Add<TCollection>(TCollection keys, Func<TKey, TValue> valueFunc) where TCollection : IEnumerable<TKey> {
      foreach (var key in keys) Add(key, valueFunc(key));
    }
  }
  [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix",
    Justification = "This is conceptually a dictionary but has different lookup behavior")]
  public class LaTeXCommandDictionary<TValue> : ProxyAdder<string, TValue>, IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TValue>> {
    public delegate Result<(TValue Result, int SplitIndex)> DefaultDelegate(ReadOnlySpan<char> consume);

    public LaTeXCommandDictionary(DefaultDelegate @default,
      DefaultDelegate defaultForCommands, Action<string, TValue>? added = null) : base(added) {
      this.@default = @default;
      this.defaultForCommands = defaultForCommands;
      Added += (key, value) => {
        if (key.AsSpan().StartsWithInvariant(@"\"))
          if (SplitCommand(key.AsSpan()) != key.Length - 1)
            commands.Add(key, value);
          else throw new ArgumentException("Key is unreachable: " + key, nameof(key));
        else nonCommands.Add(key.AsMemory(), value);
      };
    }
    readonly DefaultDelegate @default;
    readonly DefaultDelegate defaultForCommands;

    readonly PatriciaTrie<char, TValue> nonCommands = new PatriciaTrie<char, TValue>();
    readonly Dictionary<string, TValue> commands = new Dictionary<string, TValue>();

    public void Clear() {
      nonCommands.Clear();
      commands.Clear();
    }
    public bool ContainsKey(ReadOnlySpan<char> key) =>
      nonCommands.ContainsKey(key) || commands.ContainsKey(key.ToString());

    public IEnumerator<KeyValuePair<ReadOnlyMemory<char>, TValue>> GetEnumerator() =>
      nonCommands.Select(kvp => new KeyValuePair<ReadOnlyMemory<char>, TValue>(kvp.Key, kvp.Value))
      .Concat(commands.Select(kvp => new KeyValuePair<ReadOnlyMemory<char>, TValue>(kvp.Key.AsMemory(), kvp.Value)))
      .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Lookup a Dictionary<char, TValue> with a ReadOnlySpan<char> in the future:
    // https://github.com/dotnet/runtime/issues/27229
    public bool Remove(ReadOnlySpan<char> key) =>
      key.StartsWithInvariant(@"\")
      ? commands.Remove(key.ToString())
      : nonCommands.Remove(key);

    static int SplitCommand(ReadOnlySpan<char> consume) {
      // https://stackoverflow.com/questions/29217603/extracting-all-latex-commands-from-a-latex-code-file#comment47075515_29218404
      static bool IsEnglishAlphabetOrAt(char c) => 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '@';

      System.Diagnostics.Debug.Assert(consume[0] == '\\');
      var splitIndex = 1;
      if (splitIndex < consume.Length)
        if (IsEnglishAlphabetOrAt(consume[splitIndex])) {
          do splitIndex++; while (splitIndex < consume.Length && IsEnglishAlphabetOrAt(consume[splitIndex]));
          if (splitIndex < consume.Length)
            switch (consume[splitIndex]) {
              case '*':
              case '=':
              case '\'':
                splitIndex++;
                break;
            }
        } else splitIndex++;
      return splitIndex;
    }
    public Result<(TValue Result, int SplitIndex)> TryLookup(ReadOnlySpan<char> consume) {
      if (consume.IsEmpty) throw new ArgumentException("There are no characters to read.", nameof(consume));
      if (consume.StartsWithInvariant(@"\")) {
        var splitIndex = SplitCommand(consume);
        var lookup = consume.Slice(0, splitIndex);
        while (splitIndex < consume.Length && char.IsWhiteSpace(consume[splitIndex]))
          splitIndex++;
        return commands.TryGetValue(lookup.ToString(), out var result)
               ? Result.Ok((result, splitIndex))
               : defaultForCommands(lookup);
      } else
        return nonCommands.TryLookup(consume) is { } result ? result : @default(consume);
    }
  }

  //https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
  public class BiDictionary<TFirst, TSecond>
    : ProxyAdder<TFirst, TSecond>, IDictionary<TFirst, TSecond>, IReadOnlyDictionary<TFirst, TSecond> {
    public BiDictionary() : base() =>
      Added += (first, second) => {
        switch (firstToSecond.ContainsKey(first), secondToFirst.ContainsKey(second)) {
          case (true, true):
            firstToSecond.Add(first, second); // Throw: key and value both exist
            break;
          case (true, false):
            secondToFirst.Add(second, first);
            break;
          case (false, true):
            firstToSecond.Add(first, second);
            break;
          case (false, false):
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
            break;
        }
      };

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