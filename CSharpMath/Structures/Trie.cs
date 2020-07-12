using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath {
  partial class Extensions {
    /// <returns>When (@this, other) == ("1234567", "1234abc"), (commonHead, thisRest, otherRest) == ("1234", "567", "abc")</returns>
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
    /// <returns>When (@this, other) == ("1234567", "1234abc"), (commonHead, thisRest, otherRest) == ("1234", "567", "abc")</returns>
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