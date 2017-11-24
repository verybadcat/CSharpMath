using System;
using CSharpMath;

namespace CSharpMath
{
  public class BooleanAttribute: Attribute<bool>
  {
    public override bool PayloadBool(bool defaultValue) => Payload;

    public override GenericAttribute CloneAttribute() {
      return new BooleanAttribute(this.Payload);
    }
    public BooleanAttribute(bool payload): base(payload) { }
    public BooleanAttribute(string encodedPayload) : this(BoolExtensions.TryParse(encodedPayload))
    {
    }
  }
}

