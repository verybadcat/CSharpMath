using System;
using CSharpMath.Maths;

namespace CSharpMath
{
  public class IntAttribute: Attribute<int>
  {
    public IntAttribute(int payload) : base(payload)
    {
    }

    public static IntAttribute FromString(string encodedPayload) {
      int? parsePay = IntAdditions.TryParseQ(encodedPayload);
      IntAttribute r = parsePay.Construct(n => new IntAttribute(n));
      return r;
    }

    public override GenericAttribute CloneAttribute() {
      return new IntAttribute(this.Payload);
    }

    public override bool PayloadsAreEqual(int payload1, int payload2) {
      return payload1 == payload2;
    }
    public override int? PayloadIntQ {
      get {
        return this.Payload;
      }
    }
    public override bool PayloadBool(bool defaultValue = false) => Payload == 0 ? false : true;
    public override int PayloadInt(int defaultValue) {
      return this.Payload;
    }
    public override double PayloadDouble(double defaultValue) {
      return this.Payload;
    }
    public override float PayloadFloat(float defaultValue) {
      return this.Payload;
    }
  }
}

