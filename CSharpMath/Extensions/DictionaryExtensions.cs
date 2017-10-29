using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  internal static class DictionaryExtensions {
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) {
      TValue r = default(TValue);
      if (source != null && key != null && source.ContainsKey(key)) {
        r = source[key];
      }
      return r;
    }
  }
}
