using System;
using System.Buffers;
using System.Collections.Generic;
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
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix",
    Justification = "'Trie' is the correct data type, not 'Collection'")]
  public class PatriciaTrie<TKeyElement, TValue> : IEnumerable<KeyValuePair<ReadOnlyMemory<TKeyElement>, TValue>> {
    #region Originally TrieNodeBase<TValue>

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers",
      Justification = "ReadOnlySpan<char> is basically string but faster")]
    public IEnumerable<TValue> this[ReadOnlySpan<TKeyElement> query] =>
      query.IsEmpty ? ValuesDeep() : SearchDeep(query);
    protected IEnumerable<TValue> SearchDeep(ReadOnlySpan<TKeyElement> query) =>
      GetChildOrNull(query) is { } nextNode
      ? nextNode[query.Slice(Math.Min(query.Length, nextNode.Key.Length))]
      : Enumerable.Empty<TValue>();
    private IEnumerable<TValue> ValuesDeep() => Subtree().SelectMany(node => node.Values);
    protected IEnumerable<PatriciaTrie<TKeyElement, TValue>> Subtree() =>
      Enumerable.Repeat(this, 1).Concat(Children.Values.SelectMany(child => child.Subtree()));
    #endregion Originally TrieNodeBase<TValue>

    protected Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>> Children { get; private set; }
    protected ReadOnlyMemory<TKeyElement> Key { get; private set; }
    protected Queue<TValue> Values { get; private set; }

    public PatriciaTrie() : this(
      ReadOnlyMemory<TKeyElement>.Empty,
      new Queue<TValue>(),
      new Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>>()) { }
    protected PatriciaTrie(ReadOnlyMemory<TKeyElement> key, TValue value)
        : this(key, new Queue<TValue>(new[] { value }), new Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>>()) { }
    protected PatriciaTrie(ReadOnlyMemory<TKeyElement> key, Queue<TValue> values,
        Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>> children) {
      Values = values;
      Key = key;
      Children = children;
    }

    public void Add(ReadOnlyMemory<TKeyElement> keyRest, TValue value) {
      Key.ZipWith(keyRest, out var commonHead, out var thisRest, out var otherRest);
      switch (thisRest.Length, otherRest.Length) {
        case (0, 0):
          Values.Enqueue(value);
          break;
        case (0, _):
          if (!Children.TryGetValue(otherRest.Span[0], out var child)) {
            child = new PatriciaTrie<TKeyElement, TValue>(otherRest, value);
            Children.Add(otherRest.Span[0], child);
          } else {
            child.Add(otherRest, value);
          }
          break;
        case (_, 0): // A method called "SplitOne" in original source
          var leftChild = new PatriciaTrie<TKeyElement, TValue>(thisRest, Values, Children);

          Children = new Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>>();
          Values = new Queue<TValue>();
          Values.Enqueue(value);
          Key = commonHead;

          Children.Add(thisRest.Span[0], leftChild);
          break;
        case (_, _): // A method called "SplitTwo" in original source
          leftChild = new PatriciaTrie<TKeyElement, TValue>(thisRest, Values, Children);
          var rightChild = new PatriciaTrie<TKeyElement, TValue>(otherRest, value);

          Children = new Dictionary<TKeyElement, PatriciaTrie<TKeyElement, TValue>>();
          Values = new Queue<TValue>();
          Key = commonHead;

          TKeyElement leftKey = thisRest.Span[0];
          Children.Add(leftKey, leftChild);
          TKeyElement rightKey = otherRest.Span[0];
          Children.Add(rightKey, rightChild);
          break;
      }
    }

    protected PatriciaTrie<TKeyElement, TValue>? GetChildOrNull(ReadOnlySpan<TKeyElement> query) {
      if (Children.TryGetValue(query[0], out var child)) {
        var queryPartition = query.Slice(0, Math.Min(query.Length, child.Key.Length));
        child.Key.Span.ZipWith(queryPartition, out _, out _, out var queryRest);
        if (queryRest.Length == 0) {
          return child;
        }
      }
      return null;
    }

    public bool Remove(ReadOnlySpan<TKeyElement> keyRest) {
      Key.Span.ZipWith(keyRest, out _, out var thisRest, out var otherRest);
      switch (thisRest.Length, otherRest.Length) {
        case (0, 0) when Values.Count > 0:
          Values.Clear();
          return true;
        case (0, 0):
          return false;
        case (0, _) when GetChildOrNull(otherRest) is { } child:
          var success = child.Remove(otherRest);
          // Get rid of empty nodes
          if (success && child.Values.Count == 0 && child.Children.Count == 0)
            if (!Children.Remove(otherRest[0]))
              throw new InvalidCodePathException($"{nameof(child)} should exist in {nameof(Children)}!");
          return success;
        default:
          return false;
      }
    }

    sealed class MemorySequenceSegment : ReadOnlySequenceSegment<TKeyElement> {
      public MemorySequenceSegment(ReadOnlyMemory<TKeyElement> memory, long runningIndex) {
        Memory = memory;
        RunningIndex = runningIndex;
      }
      public new ReadOnlySequenceSegment<TKeyElement>? Next { get => base.Next; set => base.Next = value; }
    }
    // Can't use Stack<T> because it iterates from the newest element to the oldest, unlike List<T> which iterates the other way around
    private IEnumerable<KeyValuePair<ReadOnlyMemory<TKeyElement>, TValue>> ToEnumerable(List<ReadOnlyMemory<TKeyElement>> stack, int stackLength) {
      stack.Add(Key);
      stackLength += Key.Length;
      var fullKeyArray = new TKeyElement[stackLength];
      var writeIndex = 0;
      foreach (var memory in stack) {
        memory.CopyTo(fullKeyArray.AsMemory(writeIndex));
        writeIndex += memory.Length;
      }
      var fullKey = new ReadOnlyMemory<TKeyElement>(fullKeyArray);
      foreach (var value in Values)
        yield return new KeyValuePair<ReadOnlyMemory<TKeyElement>, TValue>(fullKey, value);
      foreach (var child in Children.Values)
        foreach (var element in child.ToEnumerable(stack, stackLength))
          yield return element;
      stack.RemoveAt(stack.Count - 1);
    }
    public IEnumerable<KeyValuePair<ReadOnlyMemory<TKeyElement>, TValue>> ToEnumerable() => ToEnumerable(new List<ReadOnlyMemory<TKeyElement>>(), 0);
    public IEnumerator<KeyValuePair<ReadOnlyMemory<TKeyElement>, TValue>> GetEnumerator() => ToEnumerable().GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ToEnumerable().GetEnumerator();
    public string Traversal() {
      var result = new StringBuilder();
      result.Append(Key.Span.ToString());

      string subtreeResult = string.Join(" ; ", Children.Values.Select(node => node.Traversal()).ToArray());
      if (subtreeResult.Length != 0) {
        result.Append("[").Append(subtreeResult).Append("]");
      }

      return result.ToString();
    }

    public override string ToString() =>
      $"Key: {Key}, Values: {Values.Count}, Children: {string.Join(";", Children.Keys)}";
  }
}
