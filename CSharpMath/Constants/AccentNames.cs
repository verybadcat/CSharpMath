using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Constants {
  public static class AccentNames {
    public static Dictionary<string, string> NameToValue { get; } = new Dictionary<string, string> {
      { "acute" , "\u0301"},
      { "hat" , "\u0302"},  // In our implementation hat and widehat behave the same.
      { "grave" , "\u0300"},
      { "tilde" , "\u0303"}, // In our implementation tilde and widetilde behave the same.
      { "bar" , "\u0304"},
      { "breve" , "\u0306"},
      { "dot" , "\u0307"},
      { "ddot" , "\u0308"},
      { "check" , "\u030C"},
      { "vec" , "\u20D7"},
      { "widehat" , "\u0302"},
      { "widetilde" , "\u0303"},
    };

    private static Dictionary<string, string> _valueToName;

    public static Dictionary<string, string> ValueToName {
      get {
        if (_valueToName == null) {
          _valueToName = DictionaryHelpers.BuildValueToKeyDictionary(_valueToName.Keys, key => _valueToName[key], str => true);
        }
        return _valueToName;
      }
    }
  }
}
