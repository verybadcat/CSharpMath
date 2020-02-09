using System;
using System.Collections.Generic;

namespace CSharpMath.Structures {
  public ref struct RentedArray<T> {
    public RentedArray(int length) {
      EntireArray = System.Buffers.ArrayPool<T>.Shared.Rent(length);
      Length = length;
    }
    public RentedArray(IEnumerable<T> ie, int length) {
      EntireArray = System.Buffers.ArrayPool<T>.Shared.Rent(length);
      using var enumerator = ie.GetEnumerator();
      for (int i = 0; i < length && enumerator.MoveNext(); i++)
        EntireArray[i] = enumerator.Current;
      Length = length;
    }
    public RentedArray(T item1) : this(1) {
      EntireArray[0] = item1;
    }
    public RentedArray(T item1, T item2) : this(2) {
      EntireArray[0] = item1;
      EntireArray[1] = item2;
    }
    public RentedArray(T item1, T item2, T item3) : this(3) {
      EntireArray[0] = item1;
      EntireArray[1] = item2;
      EntireArray[2] = item3;
    }
    public T[] EntireArray { get; }
    public int Length { get; }
    public ArraySegment<T> Result => new ArraySegment<T>(EntireArray, 0, Length);
    public void Dispose() => System.Buffers.ArrayPool<T>.Shared.Return(EntireArray);
  }
}
