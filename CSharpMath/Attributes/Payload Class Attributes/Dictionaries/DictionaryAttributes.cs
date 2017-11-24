using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CSharpMath
{
  public static class DictionaryAttributes
  {
    public static DictionaryAttribute Wrap(Dictionary<string, object> dict) {
      DictionaryAttribute r = new DictionaryAttribute();
      foreach(string key in dict.Keys) {
        object value = dict[key];
        r.SetObject(key, value);
      }
      return r;
    }
    public static DictionaryAttribute WrapReadOnly<T>(IReadOnlyDictionary<string, T> dict) {
      DictionaryAttribute r = new DictionaryAttribute();
      foreach (KeyValuePair<string, T> pair in dict) {
        GenericAttribute wrapContent = GenericAttributes.Wrap(pair.Value);
        string key = pair.Key;
        r[key] = wrapContent;
      }
      return r;
    }
  }
}

