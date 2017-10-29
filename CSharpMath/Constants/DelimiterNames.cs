using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Constants {
  public static class DelimiterNames {
    public static Dictionary<string, string> NameToValue { get; } = new Dictionary<string, string> {
      { ".", "" }, // . means no delimiter
      { "(", "(" },
      { ")", ")" },
      { "[", "[" },
      { "]", "]" },
      { "<", "\u2329" },
      { ">", "\u232A" },
      { "/", "/" },
      { "\\", "\\" },
      { "|", "|" },
      { "lgroup", "\u27EE" },
      { "rgroup", "\u27EF" },
      { "||", "\u2016" },
      { "Vert", "\u2016" },
      { "vert", "|" },
      { "uparrow", "\u2191" },
      { "downarrow", "\u2193" },
      { "updownarrow", "\u2195" },
      { "Uparrow", "21D1" },
      { "Downarrow", "21D3" },
      { "Updownarrow", "21D5" },
      { "backslash", "\\" },
      { "rangle", "\u232A" },
      { "langle", "\u2329" },
      { "rbrace", "}" },
      { "}", "}" },
      { "{", "{" },
      { "lbrace", "{" },
      { "lceil", "\u2308" },
      { "rceil", "\u2309" },
      { "lfloor", "\u230A" },
      { "rfloor", "\u230B" }
    };

    private static Dictionary<string, string> _valueToName = null;

    public static Dictionary<string, string> ValueToName {
      get {
        if (_valueToName == null) {
          _valueToName = DictionaryHelpers.BuildValueToKeyDictionary(_valueToName.Keys, key => _valueToName[key]);
        }
        return _valueToName;
      }
    }
  }
}
