using System;
using System.Collections.Generic;

namespace CSharpMath.Structures {
// RentedArray not supposed to be equatable
#pragma warning disable CA1815 // Override equals and operator equals on value types
  public ref struct RentedArray<T> {
#pragma warning restore CA1815 // Override equals and operator equals on value types
    public RentedArray(int length) =>
      Result = new ArraySegment<T>(System.Buffers.ArrayPool<T>.Shared.Rent(length), 0, length);
    public RentedArray(IEnumerable<T> ie, int length) : this(length) {
      using var enumerator = ie.GetEnumerator();
      for (int i = 0; i < length && enumerator.MoveNext(); i++)
        Result.Array[i] = enumerator.Current;
    }
    public RentedArray(T item1) : this(1) {
      Result.Array[0] = item1;
    }
    public RentedArray(T item1, T item2) : this(2) {
      Result.Array[0] = item1;
      Result.Array[1] = item2;
    }
    public RentedArray(T item1, T item2, T item3) : this(3) {
      Result.Array[0] = item1;
      Result.Array[1] = item2;
      Result.Array[2] = item3;
    }
    public ArraySegment<T> Result { get; }
    public void Dispose() => System.Buffers.ArrayPool<T>.Shared.Return(Result.Array);
  }
}