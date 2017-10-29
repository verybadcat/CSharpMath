using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  /// <summary>Marker interface</summary>
  public interface IMathObject {
  }

  public static class IMathObjectExtensions {
    /// Safe to call, even if one or both are null. Returns true if both are null. 
    public static bool NullCheckingEquals(this IMathObject obj1, IMathObject obj2) {
      if (obj1 == null && obj2 == null) {
        return true;
      }
      return obj1.Equals(obj2);
    }
  }
}
