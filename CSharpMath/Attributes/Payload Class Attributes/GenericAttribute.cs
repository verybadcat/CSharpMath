using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;
using Newtonsoft.Json;

namespace CSharpMath
{
  // Attribute is a system class name, so we use GenericAttribute instead.
  [JsonObject(MemberSerialization.OptIn, IsReference = true, ItemTypeNameHandling = TypeNameHandling.Auto)]
  public abstract class GenericAttribute
  {
    public virtual GenericAttribute FindDescendant(Predicate<GenericAttribute> condition) {
      if (condition(this)) {
        return this;
      } else {
        foreach(GenericAttribute child in this.GetChildren()) {
          GenericAttribute descendant = child.FindDescendant(condition);
          if (descendant!=null) {
            return descendant;
          }
        }
      }
      return null;
    }
    public virtual IEnumerable<GenericAttribute> GetChildren() {
      return Enumerable.Empty<GenericAttribute>();
    }
    public virtual TResult Accept<TResult>(IResultingAttributeVisitor<TResult> visitor) {
      TResult r = visitor.VisitGeneric(this);
      return r;
    }
    public abstract GenericAttribute CloneAttribute();
    public abstract GenericAttribute CloneOrUpdateGeneric(GenericAttribute recycleMe);
    public abstract object PayloadObject { get; }
    public abstract object ClonePayload();  // for immutable and value types, this is the same as PayloadObject
    public virtual GenericAttribute CloneAttributeButReplaceString(string replaceMe, string replacement) {
      GenericAttribute r = this.CloneAttribute();
      return r;
    }
    // <summary>DefaultValue actually defaults to new BitArray(0).</summary>
    public virtual BitArray PayloadBitArray(BitArray defaultValue = null) {
      if (defaultValue == null) {
        defaultValue = new BitArray(0);
      }
      return defaultValue;
    }
    /// <summary>All of the Payload methods take the point of view that
    /// the only casts we perform are:
    /// </summary>
    public virtual Double PayloadDouble(double defaultValue = double.NaN) {
      return defaultValue;
    }
    /// <summary>PointFAdditions.NaN may be an appropriate defaultValue</summary>
    public virtual PointF PayloadPointF(PointF defaultValue) {
      return defaultValue;
    }
    public virtual float PayloadFloat(float defaultValue = float.NaN) {
      return defaultValue;
    }
    public virtual DateTime PayloadDateTime(DateTime defaultValue) {
      return defaultValue;
    }
    /// <summary> Hack returning object because IWrapperFunction is defined at a
    /// higher level.</summary>
    public virtual int? PayloadIntQ {
      get {
        return null;
      }
    }
    public virtual string PayloadString(string defaultValue = null) {
      return defaultValue;
    }
    public virtual string PayloadString(Func<string> makeDefaultValue) {
      return makeDefaultValue();
    }
    public virtual int PayloadInt(int defaultValue) {
      return defaultValue;
    }
    public virtual byte PayloadByte(byte defaultValue = byte.MaxValue) {
      return defaultValue;
    }
    public virtual Color? PayloadColor(Color? defaultValue = null) {
      return defaultValue;
    }
    /// <summary>Don't override this; override PayloadBoolQ instead.</summary>
    public virtual bool PayloadBool(bool defaultValue = false) {
      return defaultValue;
    }
    public virtual RectangleF PayloadRectangleF(RectangleF defaultValue) {
      return defaultValue;
    }
    public virtual IDictionary<string, GenericAttribute> PayloadDictionary(IDictionary<string, GenericAttribute> defaultValue = null) {
        return defaultValue;
    }
    public abstract object ToNonAttributeObjectRecursively(bool unwrapSingleQuoteSerializingStrings);
    #region requirements
    internal virtual Requirements RequirementsForEnclosingDictionary { get { return new Requirements(); } }
    public virtual Requirements MyRequirements { get { return new Requirements(); } }
    public bool IsSatisfiedBy(Requirements req) { return this.MyRequirements.IsSatisfiedBy(req); }
    public virtual GenericAttribute TrimWithMetRequirements(Requirements met, bool unwrap) {
      return met.Satisfies(this.MyRequirements) ? this : null;
    }
    #endregion
    /// <summary>Because it will be prefixed with "key=", this method can assume that the type of the attribute is known.
    /// Because this is used for serialzation, overrides should never return null.  Returning the string "null" is acceptable.
    /// The encoding is expected to be round-trip with the string constructor of the attribute.  If the payload is a class,
    /// round-trip failures involving a null payload getting mapped to a default value are acceptable.
    /// Also, the encoding is expected never to contain a comma or a right curly brace, because we use commas as the separator 
    /// in the list of attributes, and we end it with a right curly brace</summary>
    public abstract bool PayloadIsUndefined();
    public abstract bool EqualsAttribute(GenericAttribute otherAttribute);

  }

  public static class GenericAttributeAdditions {
    public static bool AttributesAreEqual(this GenericAttribute attribute, GenericAttribute other, bool bothNullOK) {
      bool r;
      if (attribute == null) {
        r = ((other == null) && bothNullOK);
      } else {
        r = attribute.EqualsAttribute(other);
      }
      return r;
    }
    public static bool IsNullOrEmptyStringAttribute(this GenericAttribute attribute) {
      bool r = false;
      if (attribute == null) {
        r = true;
      } else {
        StringAttribute str = attribute as StringAttribute;
        if (str!=null) {
          if (string.IsNullOrEmpty(str.Payload)) {
            r = true;
          }
        }
      }
      return r;
    }
  }

  /// <summary>An entry in a dictionary of attributes.  Concrete subclasses are expected to have a string-based constructor which
  /// round-trips with the "Encoding" method.</summary>
  [JsonObject(MemberSerialization.OptIn, IsReference = true, ItemTypeNameHandling = TypeNameHandling.Auto)]
  public abstract class Attribute<TAttribute> : GenericAttribute
  {
    public override object ToNonAttributeObjectRecursively(bool unwrapSingleQuoteSerializingStrings) {
      return this.Payload;
    }

    /// <summary>This is the same as using the (currently unused) class WrapperEqualityComparers.Equivalence.</summary>
    public virtual bool PayloadsAreEqual(TAttribute payload1, TAttribute payload2) {
      var comparer = EqualityComparer<TAttribute>.Default;
      return comparer.Equals(payload1, payload2);
    }

    [JsonProperty]
    public TAttribute Payload { get; set; }
    public override object PayloadObject {
      get {
        return this.Payload;
      }
    }
    public Attribute(TAttribute value)
      : base()
    {
      this.Payload = value;
    }
    public override bool PayloadIsUndefined() {
      bool r = this.Payload == null;
      return r;
    }
    public Attribute() : this(default(TAttribute))
    {
      
    }

    public virtual TAttribute NarrowClonePayload() {
      return this.Payload;
    }

    ///<summary>Override NarrowClonePayload, not this.</summary> 
    public sealed override object ClonePayload() {
      return this.NarrowClonePayload();
    }

    public sealed override GenericAttribute CloneOrUpdateGeneric(GenericAttribute recycleMe) {
      return this.CloneOrUpdate(recycleMe);
    }

    public virtual Attribute<TAttribute> CloneOrUpdate(GenericAttribute recycleMe) {
      Attribute<TAttribute> target = recycleMe as Attribute<TAttribute>;
      if (target == null) {
        target = this.CloneAttribute() as Attribute<TAttribute>;
      } else {
        TAttribute pay = this.NarrowClonePayload();
        target.Payload = pay;
      }
      return target;
    }
    public override int GetHashCode() {
      TAttribute payload = this.Payload;
      int r = 0;
      if (payload != null) {
        r = payload.GetHashCode();
      }
      return r;
    }
    public override bool EqualsAttribute(GenericAttribute obj) {
      bool r = false;
      if (!Object.ReferenceEquals(obj, null)) {
        Type myType = this.GetType();
        Type otherType = obj.GetType();
        if (myType == otherType) {
          Attribute<TAttribute> otherAttribute = obj as Attribute<TAttribute>;
          TAttribute myPayload = this.Payload;
          TAttribute otherPayload = otherAttribute.Payload;
          if (this.PayloadsAreEqual(myPayload, otherPayload)) {
            r = true;
          }
        }
      }
      return r;
    }
  }
}
