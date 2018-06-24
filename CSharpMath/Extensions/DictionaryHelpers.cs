using System;
using System.Collections.Generic;

namespace CSharpMath {
  /// <summary>
  /// Used before the introduction of BiDictionary, currently unused
  /// </summary>
  internal static class DictionaryHelpers {
    public static bool ShouldSetValueWhenBuildingDictionary(
      Dictionary<string, string> buildingDictionary,
      string keyInBuildingDictionary,
      string proposedValue) {
      if (keyInBuildingDictionary.Length > 0) {
        if (buildingDictionary.ContainsKey(keyInBuildingDictionary)) {
          // If there are two values for the same key, choose one deterministically
          var existingValue = buildingDictionary[keyInBuildingDictionary];
          if (proposedValue.Length > existingValue.Length) {
            // keep the shorter value
            return false;
          }
          if (proposedValue.Length == existingValue.Length) {
            // keep the one that is alphabetically first
            if (proposedValue.CompareTo(existingValue) > 0) {
              return false;
            }
          }
        }
      }
      return true;
    }

    public static Dictionary<string, string> BuildValueToKeyDictionary (
      IEnumerable<string> values,
      Func <string, string> getKeyForValue,
      Func<string, bool> keepKey) {
      Dictionary<string, string> r = new Dictionary<string, string>();
      foreach (var val in values) {
        
        var key = getKeyForValue(val);
        if (keepKey(key)) {
          if (ShouldSetValueWhenBuildingDictionary(r, key, val)) {
            r[key] = val;
          }
        }
      }
      return r;
    }
  }
}
