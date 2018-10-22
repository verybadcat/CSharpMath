using System;
using System.Collections.Generic;
using System.Text;
using MethodImpl = System.Runtime.CompilerServices.MethodImplAttribute;
using Impl = System.Runtime.CompilerServices.MethodImplOptions;

namespace CSharpMath {
  public ref struct RentedArray<T> {
    public RentedArray(int length) {
      Array = System.Buffers.ArrayPool<T>.Shared.Rent(length);
      Length = length;
    }
    public RentedArray(T item1) : this(1) {
      Array[0] = item1;
    }
    public RentedArray(T item1, T item2) : this(2) {
      Array[0] = item1;
      Array[1] = item2;
    }
    public RentedArray(T item1, T item2, T item3) : this(3) {
      Array[0] = item1;
      Array[1] = item2;
      Array[2] = item3;
    }
    public T[] Array { get; }
    public int Length { get; }
    public ForEach<T> Result { [MethodImpl(Impl.AggressiveInlining)] get => Array.AsForEach(0, Length); }

    public void Return() => System.Buffers.ArrayPool<T>.Shared.Return(Array);
  }
}
