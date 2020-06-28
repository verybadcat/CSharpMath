using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CSharpMath {
  partial class Extensions {
    public static void ZipWith<T>(this ReadOnlyMemory<T> @this, ReadOnlyMemory<T> other,
      out ReadOnlyMemory<T> commonHead, out ReadOnlyMemory<T> thisRest, out ReadOnlyMemory<T> otherRest) {
      var thisSpan = @this.Span;
      var otherSpan = other.Span;
      var splitIndex = 0;
      while (
        splitIndex < thisSpan.Length
        && splitIndex < otherSpan.Length
        && otherSpan[splitIndex] is var o
        && (thisSpan[splitIndex]?.Equals(o) ?? o is null)
      ) splitIndex++;
      commonHead = @this.Slice(0, splitIndex);
      thisRest = @this.Slice(splitIndex);
      otherRest = other.Slice(splitIndex);
    }
    public static void ZipWith<T>(this ReadOnlySpan<T> @this, ReadOnlySpan<T> other,
      out ReadOnlySpan<T> commonHead, out ReadOnlySpan<T> thisRest, out ReadOnlySpan<T> otherRest) {
      var splitIndex = 0;
      while (
        splitIndex < @this.Length
        && splitIndex < other.Length
        && other[splitIndex] is var o
        && (@this[splitIndex]?.Equals(o) ?? o is null)
      ) splitIndex++;
      commonHead = @this.Slice(0, splitIndex);
      thisRest = @this.Slice(splitIndex);
      otherRest = other.Slice(splitIndex);
    }
  }
}

namespace CSharpMath.Structures {
  // Based on https://github.com/gmamaladze/trienet/tree/f0cce5f980d85e445188b3eb025821fcdb740144/TrieNet/_PatriciaTrie
  // Can't use the TrieNet NuGet package because the .NET Standard 2.0 version is not uploaded: https://github.com/gmamaladze/trienet/issues/12
  [Serializable]
  public class PatriciaTrie<TKeyElement, TValue> : PatriciaTrieNode<TKeyElement, TValue> {
    public PatriciaTrie() : base(
      ReadOnlyMemory<TKeyElement>.Empty,
      new Queue<TValue>(),
      new Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>>()) { }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers",
      Justification = "ReadOnlySpan<char> is basically string but faster")]
    public IEnumerable<TValue> this[ReadOnlySpan<TKeyElement> query] => Retrieve(query, 0);
  }
  [Serializable]
  [DebuggerDisplay("'{m_Key}'")]
  public class PatriciaTrieNode<TKeyElement, TValue> {
    #region Originally TrieNodeBase<TValue>
    protected IEnumerable<TValue> Retrieve(ReadOnlySpan<TKeyElement> query, int position) =>
      position >= query.Length ? ValuesDeep() : SearchDeep(query, position);
    protected IEnumerable<TValue> SearchDeep(ReadOnlySpan<TKeyElement> query, int position) =>
      GetChildOrNull(query, position) is { } nextNode
      ? nextNode.Retrieve(query, position + nextNode.m_Key.Length)
      : Enumerable.Empty<TValue>();
    private IEnumerable<TValue> ValuesDeep() => Subtree().SelectMany(node => node.Values());
    protected IEnumerable<PatriciaTrieNode<TKeyElement, TValue>> Subtree() =>
      Enumerable.Repeat(this, 1).Concat(Children().SelectMany(child => child.Subtree()));
    #endregion Originally TrieNodeBase<TValue>

    private Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>> m_Children;
    private ReadOnlyMemory<TKeyElement> m_Key;
    private Queue<TValue> m_Values;

    protected PatriciaTrieNode(ReadOnlyMemory<TKeyElement> key, TValue value)
        : this(key, new Queue<TValue>(new[] { value }), new Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>>()) { }
    protected PatriciaTrieNode(ReadOnlyMemory<TKeyElement> key, Queue<TValue> values,
        Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>> children) {
      m_Values = values;
      m_Key = key;
      m_Children = children;
    }

    protected IEnumerable<TValue> Values() => m_Values;
    protected IEnumerable<PatriciaTrieNode<TKeyElement, TValue>> Children() => m_Children.Values;
    protected void AddValue(TValue value) => m_Values.Enqueue(value);
    public void Add(ReadOnlyMemory<TKeyElement> keyRest, TValue value) {
      m_Key.ZipWith(keyRest, out var commonHead, out var thisRest, out var otherRest);
      switch (thisRest.Length, otherRest.Length) {
        case (0, 0):
          AddValue(value);
          break;
        case (0, _):
          GetOrCreateChild(otherRest, value);
          break;
        case (_, 0): // A method called "SplitOne" in original source
          var leftChild = new PatriciaTrieNode<TKeyElement, TValue>(thisRest, m_Values, m_Children);

          m_Children = new Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>>();
          m_Values = new Queue<TValue>();
          AddValue(value);
          m_Key = commonHead;

          m_Children.Add(thisRest.Span[0], leftChild);
          break;
        case (_, _): // A method called "SplitTwo" in original source
          leftChild = new PatriciaTrieNode<TKeyElement, TValue>(thisRest, m_Values, m_Children);
          var rightChild = new PatriciaTrieNode<TKeyElement, TValue>(otherRest, value);

          m_Children = new Dictionary<TKeyElement, PatriciaTrieNode<TKeyElement, TValue>>();
          m_Values = new Queue<TValue>();
          m_Key = commonHead;

          TKeyElement leftKey = thisRest.Span[0];
          m_Children.Add(leftKey, leftChild);
          TKeyElement rightKey = otherRest.Span[0];
          m_Children.Add(rightKey, rightChild);
          break;
      }
    }

    protected void GetOrCreateChild(ReadOnlyMemory<TKeyElement> key, TValue value) {
      if (!m_Children.TryGetValue(key.Span[0], out var child)) {
        child = new PatriciaTrieNode<TKeyElement, TValue>(key, value);
        m_Children.Add(key.Span[0], child);
      } else {
        child.Add(key, value);
      }
    }

    protected PatriciaTrieNode<TKeyElement, TValue>? GetChildOrNull(ReadOnlySpan<TKeyElement> query, int position) {
      if (m_Children.TryGetValue(query[position], out var child)) {
        var queryPartition = query.Slice(position, Math.Min(query.Length - position, child.m_Key.Length));
        child.m_Key.Span.ZipWith(queryPartition, out _, out _, out var queryRest);
        if (queryRest.Length == 0) {
          return child;
        }
      }
      return null;
    }

    public bool Remove(ReadOnlySpan<TKeyElement> keyRest) {
      m_Key.Span.ZipWith(keyRest, out _, out var thisRest, out var otherRest);
      switch (thisRest.Length, otherRest.Length) {
        case (0, 0) when m_Values.Count > 0:
          m_Values.Clear();
          return true;
        case (0, 0):
          return false;
        case (0, _) when GetChildOrNull(otherRest, 0) is { } child:
          var success = child.Remove(otherRest);
          // Get rid of empty nodes
          if (success && child.m_Values.Count == 0 && child.m_Children.Count == 0)
            if (!m_Children.Remove(otherRest[0]))
              throw new InvalidCodePathException($"{nameof(child)} should exist in {nameof(m_Children)}!");
          return success;
        default:
          return false;
      }
    }

    public string Traversal() {
      var result = new StringBuilder();
      result.Append(m_Key.Span.ToString());

      string subtreeResult = string.Join(" ; ", m_Children.Values.Select(node => node.Traversal()).ToArray());
      if (subtreeResult.Length != 0) {
        result.Append("[").Append(subtreeResult).Append("]");
      }

      return result.ToString();
    }

    public override string ToString() =>
      $"Key: {m_Key}, Values: {Values().Count()}, Children: {string.Join(";", m_Children.Keys)}";
  }
}
