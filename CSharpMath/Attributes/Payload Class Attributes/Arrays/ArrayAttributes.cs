using System;
using System.Collections.Generic;
using System.Collections;

namespace CSharpMath
{
  public static class ArrayAttributes
  {
    public static ArrayAttribute Wrap<T> (IEnumerable<T> enumerable) {
      ArrayAttribute r = new ArrayAttribute();
      foreach (T t in enumerable) {
        r.AddWrapped(t);
      }
      return r;
    }
    public static ArrayAttribute WrapGeneric(IEnumerable enumerable) {
      ArrayAttribute r = new ArrayAttribute();
      foreach(object obj in enumerable) {
        r.AddWrapped(obj);
      }
      return r;
    }
  }
}

