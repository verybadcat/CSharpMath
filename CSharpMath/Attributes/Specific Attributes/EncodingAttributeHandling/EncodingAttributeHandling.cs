using System;
using System.Collections.Generic;

namespace CSharpMath
{
  public abstract class EncodingAttributeHandling
  {
    public abstract bool ShouldEncodeAttribute(string key, GenericAttribute attribute);
  }
  public static class EncodingAttributeHandlingAdditions {
    public static bool ShouldEncodeAttribute(this EncodingAttributeHandling handling, KeyValuePair<string, GenericAttribute> pair) {
      string key = pair.Key;
      GenericAttribute value = pair.Value;
      bool r = handling.ShouldEncodeAttribute(key, value);
      return r;
    }
    /// <summary>If attributes are null, returns false.</summary>
    public static bool ShouldEncodeAny(this EncodingAttributeHandling handling, AttributeCollection attributes) {
      if (attributes!=null) {
        if (attributes.AnythingToEncode(handling)) {
          return true;
        }
      }
      return false;
    }
  }
}

