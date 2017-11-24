using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath
{
  public class DictionaryAttribute: Attribute<IDictionary<string, GenericAttribute>>, IDictionary<string, GenericAttribute>,
  IReadOnlyDictionary<string, GenericAttribute>
  {

    public IEnumerable<string>AllKeys() {
      return this.Keys;
    }
    /// <summary>Wraps the object to a GenericAttribute if needed</summary>
    public void SetObject(string key, object value) {
      if (key != null) {
        GenericAttribute attr = GenericAttributes.Wrap(value);
        this.Payload[key] = attr;
      }
    }
    public void SetEnumerable<T>(string key, IEnumerable<T> value) {
      ArrayAttribute wrapped = ArrayAttributes.Wrap(value);
      this.SetValue(key, wrapped);
    }
    public void SetValue(string key, GenericAttribute value) {
      this[key] = value;
    }
    public override TResult Accept<TResult>(IResultingAttributeVisitor<TResult> visitor) {
      TResult r = visitor.Visit(this);
      return r;
    }
    public override IDictionary<string, GenericAttribute> PayloadDictionary(IDictionary<string, GenericAttribute> defaultValue = null) {
      IDictionary<string, GenericAttribute> r = this.Payload;
      return r;
    }

    public override String ToString() {
      int nKeys = this.Keys.Count;
      switch (nKeys) {
        case 0:
          return "DictionaryAttribute 0 keys";
        case 1:
          return "DictionaryAttribute 1 key " + this.Keys.First();
        default:
          return $"DictionaryAttribute { nKeys} keys";
      }
    }

    public override Requirements MyRequirements {
      get {
        GenericAttribute attribute = this.PayloadDictionary()?.GetValueOrDefault("requires");
        string value = attribute?.PayloadString();
        Requirements r = Requirements.FromCommaSeparatedString(value);
        return r;
      }
    }

    public DictionaryAttribute() : this(new Dictionary<string, GenericAttribute>())
    {
    }

    public DictionaryAttribute(IDictionary<string, GenericAttribute> payload) : base(payload)
    {
    }

    public override bool PayloadsAreEqual(IDictionary<string, GenericAttribute> payload1, IDictionary<string, GenericAttribute> payload2) {
      int n1 = payload1.Count;
      int n2 = payload2.Count;
      bool r = false;
      if (n1 == n2) {
        r = true;
        foreach (KeyValuePair<string, GenericAttribute> pair in payload1) {
          string key = pair.Key;
          GenericAttribute value1 = pair.Value;
          GenericAttribute value2 = payload2.GetValueOrDefault(key);
          if (!value1.EqualsAttribute(value2)) {
            r = false;
            break;
          }
        }
      }
      return r;
    }

    public override IEnumerable<GenericAttribute> GetChildren() {
      return this.Payload.Values;
    }

    public override GenericAttribute CloneAttribute() {
      IDictionary<string, GenericAttribute> myContent = this.Payload;
      Dictionary<string, GenericAttribute> cloneContent = new Dictionary<string, GenericAttribute>();
      foreach (KeyValuePair<string, GenericAttribute> pair in myContent) {
        string key = pair.Key;
        GenericAttribute value = pair.Value;
        GenericAttribute cloneValue = value.CloneAttribute();
        cloneContent[key] = cloneValue;
      }
      DictionaryAttribute r = new DictionaryAttribute(cloneContent);
      return r;
    }
    public override GenericAttribute CloneAttributeButReplaceString(string replaceMe, string replacement) {
      IDictionary<string, GenericAttribute> myContent = this.Payload;
      Dictionary<string, GenericAttribute> cloneContent = new Dictionary<string, GenericAttribute>();
      foreach (KeyValuePair<string, GenericAttribute> pair in myContent) {
        string key = pair.Key;
        GenericAttribute value = pair.Value;
        GenericAttribute cloneValue = value.CloneAttributeButReplaceString(replaceMe, replacement);
        cloneContent[key] = cloneValue;
      }
      DictionaryAttribute r = new DictionaryAttribute(cloneContent);
      return r;
    }

    /// <summary>Returns null if this does not contain the key, or if the value is not
    /// a string attribute (even if it could be converted to a string or string attribute, 
    /// that won't happen.)<summary>  
    public string StringForKey(string key) {
      bool contains = this.ContainsKey(key);
      if (contains) {
        GenericAttribute obj = this[key];
        string r = obj.PayloadString();
        return r;
      } else {
        return null;
      }
    }
    private T PayloadForKey<T>(string key, Func<GenericAttribute, T, T> payloadGetter, T defaultValue) {
      bool contains = this.ContainsKey(key);
      T r = defaultValue;
      if (contains) {
        GenericAttribute obj = this[key];
        r = payloadGetter(obj, defaultValue);
      }
      return r;
    }
    private T PayloadForKey<T>(string key, Func<GenericAttribute, Func<T>, T> payloadGetter, Func<T> defaultValue) {
      bool contains = this.ContainsKey(key);
      T r;
      if (contains) {
        GenericAttribute obj = this[key];
        r = payloadGetter(obj, defaultValue);
      } else {
        r = defaultValue();
      }
      return r;
    }
    public float FloatForKey(string key, float defaultValue = float.NaN) {
      float r = this.PayloadForKey(key, (attr, f) => attr.PayloadFloat(f), defaultValue);
      return r;
    }
    /// <summary>Returns null if dictionary does not contain the key, or if node at the key
    /// does not have an integer payload.</summary>
    public int? intQForKey(string key) {
      int? r = null;
      if (this.ContainsKey(key)) {
        GenericAttribute value = this[key];
        r = value.PayloadIntQ;
      }
      return r;
    }
    public DictionaryAttribute DictionaryForKey(string key) {
      DictionaryAttribute r = null;
      bool contains = this.ContainsKey(key);
      if (contains) {
        GenericAttribute node = this[key];
        r = node as DictionaryAttribute;
      }
      return r;
    }

    public DictionaryAttribute DictionaryForKey(string key, DictionaryAttribute defaultValue) {
      DictionaryAttribute r = defaultValue;
      bool contains = this.ContainsKey(key);
      if (contains) {
        r = this[key] as DictionaryAttribute ?? defaultValue;
      }
      return r;
    }


    public double DoubleForKey(string key, double defaultValue) {
      double r = this.PayloadForKey(key, (attr, x) => attr.PayloadDouble(x), defaultValue);
      return r;
    }

    public void SetBool(string key, bool value) {
      BooleanAttribute attribute = new BooleanAttribute(value);
      this[key] = attribute;
    }

    public void SetFloat(string key, float value) {
      FloatAttribute attribute = new FloatAttribute(value);
      this[key] = attribute;
    }

    public void SetString(string key, string value) {
      StringAttribute attribute = new StringAttribute(value);
      this[key] = attribute;
    }

    public void SetInteger(string key, int value) {
      IntAttribute attribute = new IntAttribute(value);
      this[key] = attribute;
    }

    public void SetByte(string key, byte value) {
      ByteAttribute attribute = new ByteAttribute(value);
      this[key] = attribute;
    }

    public void SetDouble(string key, double value) {
      DoubleAttribute attribute = new DoubleAttribute(value);
      this[key] = attribute;
    }
    public void SetDate(string key, DateTime value) {
      DateAttribute date = new DateAttribute(value);
      this[key] = date;
    }

    public void Add(string key, GenericAttribute value) {
      this.Payload.Add(key, value);
    }

    public bool ContainsKey(string key) {
      return this.Payload.ContainsKey(key);
    }

    public bool Remove(string key) {
      bool r = this.Payload.Remove(key);
      return r;
    }

    public bool TryGetValue(string key, out GenericAttribute value) {
      return this.Payload.TryGetValue(key, out value);
    }

    public GenericAttribute this [string index] {
      get {
        return this.Payload[index];
      }
      set {
        this.Payload[index] = value;
      }
    }

    public ICollection<string> Keys {
      get {
        return this.Payload.Keys;
      }
    }

    IEnumerable<string> IReadOnlyDictionary<string, GenericAttribute>.Keys {
      get {
        ICollection<string> r = this.Payload.Keys;
        return r;
      }
    }

    public ICollection<GenericAttribute> Values {
      get {
        return this.Payload.Values;
      }
    }

    IEnumerable<GenericAttribute> IReadOnlyDictionary<string, GenericAttribute>.Values {
      get {
        return this.Payload.Values;
      }
    }
      
    public string StringForKey(string key, string defaultValue) {
      return this.PayloadForKey(key, (attr, s) => attr.PayloadString(s), defaultValue);
    }
    public string StringForKey(string key, Func<string> getDefaultValue) {
      return this.PayloadForKey(key, (attr, fs) => attr.PayloadString(fs), getDefaultValue);
    }
    public int IntForKey(string key, int defaultValue) {
      return this.PayloadForKey(key, (attr, n) => attr.PayloadInt(defaultValue), defaultValue);
    }
    public byte ByteForKey(string key, byte defaultValue) {
      return this.PayloadForKey(key, (attr, n) => attr.PayloadByte(defaultValue), defaultValue);
    }
    public bool BoolForKey(string key, bool defaultValue) {
      return this.PayloadForKey(key, (attr, b) => attr.PayloadBool(defaultValue), defaultValue);
    }
    public DateTime DateTimeForKey(string key, DateTime defaultValue) {
      return this.PayloadForKey(key, (attr, dt) => attr.PayloadDateTime(defaultValue), defaultValue);
    }
    public GenericAttribute ValueForKey(string key, Func<GenericAttribute> onFailure) {
      GenericAttribute r = null;
      if (this.ContainsKey(key)) {
        r = this[key];
      } 
      if (r == null) {
        r = onFailure();
      }
      return r;
    }


    #region ICollection implementation

    public void Add(KeyValuePair<string, GenericAttribute> item) {
      this.Payload.Add(item);
    }

    public void Clear() {
      this.Payload.Clear();
    }

    public bool Contains(KeyValuePair<string, GenericAttribute> item) {
      return this.Payload.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, GenericAttribute>[] array, int arrayIndex) {
      this.Payload.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, GenericAttribute> item) {
      return this.Payload.Remove(item);
    }

    public int Count {
      get {
        return this.Payload.Count;
      }
    }

    public bool IsReadOnly {
      get {
        return this.Payload.IsReadOnly;
      }
    }

    public override object ToNonAttributeObjectRecursively(bool unwrapSingleQuoteSerializingStrings) {
      Dictionary<string, object> r = this.ToDictionary(true, unwrapSingleQuoteSerializingStrings);
      return r;
    }


    #endregion

    #region IEnumerable implementation

    public IEnumerator<KeyValuePair<string, GenericAttribute>> GetEnumerator() {
      return this.Payload.GetEnumerator();
    }

    #endregion

    #region IEnumerable implementation

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.Payload.GetEnumerator();
    }

    #endregion
  }

  public static class DictionaryAttributeAdditions
  {
    public static Dictionary<string, object> ToDictionary(this DictionaryAttribute attribute, bool recursively, bool unwrapSingleQuoteStrings = false) {
      Dictionary<string, object> r = null;
      if (attribute != null) {
        r = new Dictionary<string, object>();
        foreach (string key in attribute.Keys) {
          GenericAttribute content = attribute[key];
          object value;
          if (recursively) {
            value = content?.ToNonAttributeObjectRecursively(unwrapSingleQuoteStrings);
          } else {
            value = content?.PayloadObject;
          }
          if (value != null) {
            r[key] = value;
          }
        }
      }
      return r;
    }
  }
}

