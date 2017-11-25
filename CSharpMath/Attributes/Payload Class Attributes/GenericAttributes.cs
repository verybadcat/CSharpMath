using System;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace CSharpMath {
  public static class GenericAttributes
  {
    /// <summary>If the payload is a GenericAttribute, we actually don't
    /// re-wrap. As of this writing (8/2016), that behavior never comes up,
    /// however.</summary>
    public static GenericAttribute Wrap(object payload) {
      try {
        if (payload == null) {
          return null;
        }
        else if (payload is int) {
          return new IntAttribute((int)payload);
        } else if (payload is long) {
          long lPayload = (long)payload;
          if (lPayload <= (long)int.MaxValue && lPayload >= (long)int.MinValue) {
            return new IntAttribute((int)lPayload);
          } else {
            return new StringAttribute(payload.ToString());
          }
        } else if (payload is double) {
          return new DoubleAttribute((double)payload);
        } else if (payload is string) {
          return new StringAttribute((string)payload);
        } else if (payload is Dictionary<string, object>) {
          return DictionaryAttributes.Wrap(payload as Dictionary<string, object>);
        } else if (payload is bool) {
          return new BooleanAttribute((bool)payload);
        } else if (payload is DateTime) {
          return new DateAttribute((DateTime)payload);
        }
        else if (payload is GenericAttribute) {
          return payload as GenericAttribute;
        } else if (payload is IReadOnlyDictionary<string, object>) {
          return DictionaryAttributes.WrapReadOnly(payload as IReadOnlyDictionary<string, object>);
        } else if (payload is JValue) {
          JValue val = payload as JValue;
          object obj = val.Value;
          return GenericAttributes.Wrap(obj);
        } else if (payload is JObject) {
          JObject j = payload as JObject;
          Dictionary<string, object> dict = j.ToObject<Dictionary<string, object>>();
          if (dict == null) {
            throw new InvalidOperationException();
          } else {
            return GenericAttributes.Wrap(dict);
          }
        } else if (payload is IEnumerable) {
          return ArrayAttributes.WrapGeneric(payload as IEnumerable);
        } else {
          throw new InvalidOperationException();
        }
      } catch (Exception) {
        return new StringAttribute(payload.ToString());
      }
    }

  }
}

