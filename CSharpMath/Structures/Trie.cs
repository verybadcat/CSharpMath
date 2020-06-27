using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using StringPartition = System.ReadOnlyMemory<char>;

namespace CSharpMath {
  partial class Extensions {
    public static Structures.SplitResult Split(this StringPartition @this, int splitAt) {
      var head = @this.Slice(0, splitAt);
      var rest = @this.Slice(splitAt);
      return new Structures.SplitResult(head, rest);
    }
    public static Structures.ZipResult ZipWith(this StringPartition @this, StringPartition other) {
      int splitIndex = 0;
      var thisEnumerator = @this.Span.GetEnumerator();
      var otherEnumerator = other.Span.GetEnumerator();
      while (thisEnumerator.MoveNext() && otherEnumerator.MoveNext()) {
        if (thisEnumerator.Current != otherEnumerator.Current) {
          break;
        }
        splitIndex++;
      }

      var thisSplitted = @this.Split(splitIndex);
      var otherSplitted = other.Split(splitIndex);

      StringPartition commonHead = thisSplitted.Head;
      StringPartition restThis = thisSplitted.Rest;
      StringPartition restOther = otherSplitted.Rest;
      return new Structures.ZipResult(commonHead, restThis, restOther);
    }
  }
}

namespace CSharpMath.Structures {
  // Based on https://github.com/gmamaladze/trienet/tree/f0cce5f980d85e445188b3eb025821fcdb740144/TrieNet/_PatriciaTrie
  // Can't use the TrieNet NuGet package because the .NET Standard 2.0 version is not uploaded: https://github.com/gmamaladze/trienet/issues/12
  public enum MatchKind {
    ExactMatch,
    Contains,
    IsContained,
    Partial,
  }
  [Serializable]
  public class PatriciaTrie<TValue> : PatriciaTrieNode<TValue> {
    public PatriciaTrie() : base(
      StringPartition.Empty,
      new Queue<TValue>(),
      new Dictionary<char, PatriciaTrieNode<TValue>>()) { }
    public IEnumerable<TValue> this[string query] => Retrieve(query, 0);
    public void Add(string key, TValue value) =>
      Add((key ?? throw new ArgumentNullException(nameof(key))).AsMemory(), value);
    private protected override void Add(StringPartition keyRest, TValue value) => GetOrCreateChild(keyRest, value);
  }
  [Serializable]
  [DebuggerDisplay("'{m_Key}'")]
  public class PatriciaTrieNode<TValue> {
    #region Originally TrieNodeBase<TValue>
    protected IEnumerable<TValue> Retrieve(string query, int position) =>
      EndOfString(position, query) ? ValuesDeep() : SearchDeep(query, position);
    protected IEnumerable<TValue> SearchDeep(string query, int position) =>
      GetChildOrNull(query, position) is { } nextNode
      ? nextNode.Retrieve(query, position + nextNode.KeyLength)
      : Enumerable.Empty<TValue>();
    private static bool EndOfString(int position, string text) => position >= text.Length;
    private IEnumerable<TValue> ValuesDeep() => Subtree().SelectMany(node => node.Values());
    protected IEnumerable<PatriciaTrieNode<TValue>> Subtree() =>
      Enumerable.Repeat(this, 1).Concat(Children().SelectMany(child => child.Subtree()));
    #endregion Originally TrieNodeBase<TValue>

    private Dictionary<char, PatriciaTrieNode<TValue>> m_Children;
    private StringPartition m_Key;
    private Queue<TValue> m_Values;

    protected PatriciaTrieNode(StringPartition key, TValue value)
        : this(key, new Queue<TValue>(new[] { value }), new Dictionary<char, PatriciaTrieNode<TValue>>()) { }
    protected PatriciaTrieNode(StringPartition key, Queue<TValue> values,
        Dictionary<char, PatriciaTrieNode<TValue>> children) {
      m_Values = values;
      m_Key = key;
      m_Children = children;
    }

    protected int KeyLength => m_Key.Length;
    protected IEnumerable<TValue> Values() => m_Values;
    protected IEnumerable<PatriciaTrieNode<TValue>> Children() => m_Children.Values;
    protected void AddValue(TValue value) => m_Values.Enqueue(value);
    private protected virtual void Add(StringPartition keyRest, TValue value) {
      ZipResult zipResult = m_Key.ZipWith(keyRest);
      switch (zipResult.MatchKind) {
        case MatchKind.ExactMatch:
          AddValue(value);
          break;
        case MatchKind.IsContained:
          GetOrCreateChild(zipResult.OtherRest, value);
          break;
        case MatchKind.Contains:
          SplitOne(zipResult, value);
          break;
        case MatchKind.Partial:
          SplitTwo(zipResult, value);
          break;
      }
    }
    private void SplitOne(ZipResult zipResult, TValue value) {
      var leftChild = new PatriciaTrieNode<TValue>(zipResult.ThisRest, m_Values, m_Children);

      m_Children = new Dictionary<char, PatriciaTrieNode<TValue>>();
      m_Values = new Queue<TValue>();
      AddValue(value);
      m_Key = zipResult.CommonHead;

      m_Children.Add(zipResult.ThisRest.Span[0], leftChild);
    }

    private void SplitTwo(ZipResult zipResult, TValue value) {
      var leftChild = new PatriciaTrieNode<TValue>(zipResult.ThisRest, m_Values, m_Children);
      var rightChild = new PatriciaTrieNode<TValue>(zipResult.OtherRest, value);

      m_Children = new Dictionary<char, PatriciaTrieNode<TValue>>();
      m_Values = new Queue<TValue>();
      m_Key = zipResult.CommonHead;

      char leftKey = zipResult.ThisRest.Span[0];
      m_Children.Add(leftKey, leftChild);
      char rightKey = zipResult.OtherRest.Span[0];
      m_Children.Add(rightKey, rightChild);
    }

    protected void GetOrCreateChild(StringPartition key, TValue value) {
      if (!m_Children.TryGetValue(key.Span[0], out var child)) {
        child = new PatriciaTrieNode<TValue>(key, value);
        m_Children.Add(key.Span[0], child);
      } else {
        child.Add(key, value);
      }
    }

    protected PatriciaTrieNode<TValue>? GetChildOrNull(string query, int position) {
      if (query == null) throw new ArgumentNullException(nameof(query));
      if (m_Children.TryGetValue(query[position], out var child)) {
        var queryPartition = query.AsMemory(position, Math.Min(query.Length - position, child.m_Key.Length));
        if (child.m_Key.Span.StartsWith(queryPartition.Span)) {
          return child;
        }
      }
      return null;
    }

    public string Traversal() {
      var result = new StringBuilder();
      result.Append(m_Key);

      string subtreeResult = string.Join(" ; ", m_Children.Values.Select(node => node.Traversal()).ToArray());
      if (subtreeResult.Length != 0) {
        result.Append("[").Append(subtreeResult).Append("]");
      }

      return result.ToString();
    }

    public override string ToString() =>
      $"Key: {m_Key}, Values: {Values().Count()}, Children:{string.Join(";", m_Children.Keys)}";
  }
  [Serializable]
  public readonly struct SplitResult : IEquatable<SplitResult> {
    public SplitResult(StringPartition head, StringPartition rest) {
      Head = head;
      Rest = rest;
    }

    public StringPartition Rest { get; }
    public StringPartition Head { get; }
    public bool Equals(SplitResult other) => Head.Span.SequenceEqual(other.Head.Span) && Rest.Span.SequenceEqual(other.Rest.Span);
    public override bool Equals(object obj) => obj is SplitResult result && Equals(result);
    public override int GetHashCode() => unchecked((Head.GetHashCode() * 397) ^ Rest.GetHashCode());
    public static bool operator ==(SplitResult left, SplitResult right) => left.Equals(right);
    public static bool operator !=(SplitResult left, SplitResult right) => !(left == right);
  }
  [Serializable]
  [DebuggerDisplay("Head: '{CommonHead}', This: '{ThisRest}', Other: '{OtherRest}', Kind: {MatchKind}")]
  public readonly struct ZipResult : IEquatable<ZipResult> {
    public ZipResult(StringPartition commonHead, StringPartition thisRest, StringPartition otherRest) {
      CommonHead = commonHead;
      ThisRest = thisRest;
      OtherRest = otherRest;
    }
    public MatchKind MatchKind =>
      (ThisRest.Length, OtherRest.Length) switch
      {
        (0, 0) => MatchKind.ExactMatch,
        (0, _) => MatchKind.IsContained,
        (_, 0) => MatchKind.Contains,
        (_, _) => MatchKind.Partial,
      };
    public StringPartition OtherRest { get; }
    public StringPartition ThisRest { get; }
    public StringPartition CommonHead { get; }
    public bool Equals(ZipResult other) =>
      CommonHead.Span.SequenceEqual(other.CommonHead.Span)
      && OtherRest.Span.SequenceEqual(other.OtherRest.Span)
      && ThisRest.Span.SequenceEqual(other.ThisRest.Span);
    public override bool Equals(object obj) => obj is ZipResult result && Equals(result);
    public override int GetHashCode() => (CommonHead, OtherRest, ThisRest).GetHashCode();
    public static bool operator ==(ZipResult left, ZipResult right) => left.Equals(right);
    public static bool operator !=(ZipResult left, ZipResult right) => !(left == right);
  }
}
