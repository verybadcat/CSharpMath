using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  public static class AnyType {
    public static bool SafeEquals(object obj1, object obj2) {
      if (obj1 == null && obj2 == null) {
        return true;
      }
      if (obj1 == null || obj2 == null) {
        return false;
      }
      return (obj1.Equals(obj2));
    }
  }
}
