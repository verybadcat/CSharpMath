using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering.Drawing {
  //MIT, 2014-present, WinterDev
  //----------------------------------------------------------------------------
  // Anti-Grain Geometry - Version 2.4
  // Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
  //
  // C# Port port by: Lars Brubaker
  //                  larsbrubaker@gmail.com
  // Copyright (C) 2007
  //
  // Permission to copy, use, modify, sell and distribute this software 
  // is granted provided this copyright notice appears in all copies. 
  // This software is provided "as is" without express or implied
  // warranty, and with no claim as to its suitability for any purpose.
  //
  //----------------------------------------------------------------------------
  // Contact: mcseem@antigrain.com
  //          mcseemagg@yahoo.com
  //          http://www.antigrain.com
  //---------------------------------------------------------------------------- 

  public class DrawingList<T> {
    public DrawingList() { }
    public DrawingList(int cap) => Allocate(cap, 0);
    public DrawingList(DrawingList<T> srcCopy, int plusSize) {
      Allocate(srcCopy.AllocatedSize, srcCopy.AllocatedSize + plusSize);
      if (srcCopy.Count != 0) {
        srcCopy.Array.CopyTo(Array, 0);
      }
    }
    public void RemoveLast() {
      if (Count != 0) {
        Count--;
      }
    }
    public int Count { get; private set; }
    public T[] Array { get; private set; } = new T[0];

    public int AllocatedSize => Array.Length;

    public void Clear() => Count = 0;

    // Set new capacity. All data is lost, size is set to zero.
    public void Clear(int newCapacity) => Clear(newCapacity, 0);
    public void Clear(int newCapacity, int extraTail) {
      Count = 0;
      if (newCapacity > AllocatedSize) {
        Array = null;
        int sizeToAllocate = newCapacity + extraTail;
        if (sizeToAllocate != 0) {
          Array = new T[sizeToAllocate];
        }
      }
    }
    // Allocate n elements. All data is lost, 
    // but elements can be accessed in range 0...size-1. 
    public void Allocate(int size) => Allocate(size, 0);

    void Allocate(int size, int extraTail) {
      Clear(size, extraTail);
      Count = size;
    }

    /// <summary>
    ///  Resize keeping the content
    /// </summary>
    /// <param name="newSize"></param>
    public void AdjustSize(int newSize) {
      if (newSize > Count) {
        if (newSize > AllocatedSize) {
          //create new array and copy data to that 
          var newArray = new T[newSize];
          if (Array != null) {
            for (int i = Array.Length - 1; i >= 0; --i) {
              newArray[i] = Array[i];
            }
          }
          Array = newArray;
        }
      }
    }

    public void Zero() {
      for (int i = Array.Length - 1; i >= 0; --i) {
        Array[i] = default;
      }
    }

    public virtual void AddVertex(T v) {
      if (Array.Length < (Count + 1)) {
        if (Count < 100000) {
          AdjustSize(Count + (Count / 2) + 16);
        } else {
          AdjustSize(Count + Count / 4);
        }
      }
      Array[Count++] = v;
    }

    public T this[int i] {
      get => Array[i];
      set => this.Array[i] = value;
    }
    public void SetData(int index, T data) =>
      this.Array[index] = data;
  }
}