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
    public ProxyAdder(Action<TKey, TValue>? extraCommandToPerformWhenAdding = null) => Added += extraCommandToPerformWhenAdding;
    public event Action<TKey, TValue>? Added;
    public void Add(TKey key1, TValue value) => Added?.Invoke(key1, value);
    public void Add(TKey key1, TKey key2, TValue value) {
      Add(key1, value); Add(key2, value);
    }
    public void Add(TKey key1, TKey key2, TKey key3, TValue value) {
      Add(key1, value); Add(key2, value); Add(key3, value);
    }
    public void Add(TKey key1, TKey key2, TKey key3, TKey key4, TValue value) {
      Add(key1, value); Add(key2, value); Add(key3, value); Add(key4, value);
    }
    public void Add<TCollection>(TCollection keys, TValue value) where TCollection : IEnumerable<TKey> {
      foreach (var key in keys) Add(key, value);
    }
    public void Add<TCollection>(TCollection keys, Func<TKey, TValue> valueFunc) where TCollection : IEnumerable<TKey> {
      foreach (var key in keys) Add(key, valueFunc(key));
    }
  }
  class DescendingStringLengthIComparer<TValue> : IComparer<(string NonCommand, TValue Value)> {
    public int Compare((string NonCommand, TValue Value) x, (string NonCommand, TValue Value) y) {
      if (x.NonCommand.Length > y.NonCommand.Length) { return -1; }
      else if (x.NonCommand.Length < y.NonCommand.Length) { return 1; }
      else { return string.CompareOrdinal(x.NonCommand, y.NonCommand); }
    }
  }

  /// <summary>
  ///  A dictionary-based helper where the keys are classes of LaTeX <see cref="string"/>s, with special treatment
  ///  for commands (starting "\"). The start of an inputted <see cref="Span{Char}"/> is parsed, and an arbitrary object
  ///  <typeparamref name="TValue"/> is returned, along with the number of matching characters. Processing is based on
  ///  dictionary lookup with fallack to specified default functions for command and non-commands when lookup fails.
  ///  For non-commands, dictionary lookup finds the longest matching non-command.
  /// </summary>
  [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix",
    Justification = "This is conceptually a dictionary but has different lookup behavior")]
  public class LaTeXCommandDictionary<TValue> : ProxyAdder<string, TValue>, IEnumerable<KeyValuePair<string, TValue>> {
    public delegate Result<(TValue Result, int SplitIndex)> DefaultDelegate(ReadOnlySpan<char> consume);

    public LaTeXCommandDictionary(DefaultDelegate defaultParser,
      DefaultDelegate defaultParserForCommands, Action<string, TValue>? extraCommandToPerformWhenAdding = null) : base(extraCommandToPerformWhenAdding) {
      this.defaultParser = defaultParser;
      this.defaultParserForCommands = defaultParserForCommands;
      Added += (key, value) => {
        if (key.AsSpan().StartsWithInvariant(@"\"))
          if (SplitCommand(key.AsSpan()) != key.Length - 1)
            commands.Add(key, value);
          else throw new ArgumentException("Key is unreachable: " + key, nameof(key));
        else nonCommands.Add((key, value));
      };
    }
    readonly DefaultDelegate defaultParser;
    readonly DefaultDelegate defaultParserForCommands;

    readonly SortedSet<(string NonCommand, TValue Value)> nonCommands =
      new SortedSet<(string NonCommand, TValue Value)>(new DescendingStringLengthIComparer<TValue>());
    readonly Dictionary<string, TValue> commands = new Dictionary<string, TValue>();

    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() =>
      nonCommands.Select(t => new KeyValuePair<string, TValue>(t.NonCommand, t.Value))
      .Concat(commands).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Finds the number of characters corresponding to a LaTeX command at the beginning of chars.</summary>
    static int SplitCommand(ReadOnlySpan<char> chars) {
      // Note on '@': https://stackoverflow.com/questions/29217603/extracting-all-latex-commands-from-a-latex-code-file#comment47075515_29218404
      static bool IsEnglishAlphabetOrAt(char c) => 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '@';

      System.Diagnostics.Debug.Assert(chars[0] == '\\');
      var splitIndex = 1;
      if (splitIndex < chars.Length)
        if (IsEnglishAlphabetOrAt(chars[splitIndex])) {
          do splitIndex++; while (splitIndex < chars.Length && IsEnglishAlphabetOrAt(chars[splitIndex]));
          if (splitIndex < chars.Length)
            switch (chars[splitIndex]) {
              case '*':
              case '=':
              case '\'':
                splitIndex++;
                break;
            }
        } else splitIndex++;
      return splitIndex;
    }
    /// <summary>Tries to find a command at the beginning of chars, returning the Value corresponding to the command Key, and the length of the command.</summary>
    public Result<(TValue Result, int SplitIndex)> TryLookup(ReadOnlySpan<char> chars) {
      if (chars.IsEmpty) throw new ArgumentException("There are no characters to read.", nameof(chars));
      if (chars.StartsWithInvariant(@"\")) {
        var splitIndex = SplitCommand(chars);
        var lookup = chars.Slice(0, splitIndex);
        while (splitIndex < chars.Length && char.IsWhiteSpace(chars[splitIndex]))
          splitIndex++;
        return commands.TryGetValue(lookup.ToString(), out var result)
               ? Result.Ok((result, splitIndex))
               : defaultParserForCommands(lookup);
      } else
        return TryLookupNonCommand(chars);
    }
    Result<(TValue Result, int SplitIndex)> TryLookupNonCommand(ReadOnlySpan<char> chars) {
      foreach ((string NonCommand, TValue Value) in nonCommands) {
        if (chars.StartsWith(NonCommand.AsSpan(), StringComparison.Ordinal)) {
          return Result.Ok((Value, NonCommand.Length)); }
      }
      return defaultParser(chars);
    }
  }

  // Taken from https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary/255638#255638
  /// <summary>
  /// Represents a many to one relationship between TFirsts and TSeconds,
  /// allowing fast lookup of the first TFirst corresponding to any TSecond,
  /// in addition to the usual lookup of a TSeconds by a TFirst.
  /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1010 // Collections should implement generic interface
  public class AliasBiDictionary<TFirst, TSecond>
#pragma warning restore CA1010 // Collections should implement generic interface
#pragma warning restore CA1710 // Identifiers should have correct suffix
    : ProxyAdder<TFirst, TSecond>
    where TFirst: IEquatable<TFirst> {
    public AliasBiDictionary(Action<TFirst, TSecond>? extraCommandToPerformWhenAdding = null) : base(extraCommandToPerformWhenAdding) =>
      Added += (first, second) => {
        switch (firstToSecond.ContainsKey(first), secondToFirst.ContainsKey(second)) {
          case (true, _):
            throw new Exception("Key already exists in AliasBiDictionary.");
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
    public Dictionary<TFirst, TSecond>.Enumerator GetEnumerator() =>
      firstToSecond.GetEnumerator();
    public bool RemoveByFirst(TFirst first) {
      bool exists = firstToSecond.TryGetValue(first, out var svalue);
      if (exists) {
        firstToSecond.Remove(first);
        // if first is currently mapped to from svalue,
        // then try to reconnect svalue to another TFirst mapping to it;
        // otherwise delete the svalue record in secondToFirst
        if (secondToFirst[svalue].Equals(first)) {
          TFirst[] otherFirsts =
            firstToSecond
            .Where(kvp => EqualityComparer<TSecond>.Default.Equals(kvp.Value,svalue))
            .Select(kvp => kvp.Key).ToArray();
          if (otherFirsts.IsEmpty()) { secondToFirst.Remove(svalue); } else { secondToFirst[svalue] = otherFirsts[0]; }
        }
      }
      return exists;
    }
    public bool RemoveBySecond(TSecond second) {
      bool exists = secondToFirst.TryGetValue(second, out var _);
      if (exists) {
        secondToFirst.Remove(second);
        // Remove all TFirsts pointing to second
        var firsts =
          firstToSecond
          .Where(kvp => EqualityComparer<TSecond>.Default.Equals(kvp.Value,second))
          .Select(kvp => kvp.Key).ToArray();
        foreach (TFirst first in firsts) { firstToSecond.Remove(first); };
      }
      return exists;
    }
    public IReadOnlyDictionary<TFirst, TSecond> FirstToSecond => firstToSecond;
    public IReadOnlyDictionary<TSecond, TFirst> SecondToFirst => secondToFirst;
  }
}