using System;

namespace CSharpMath
{
  public class IntAttribute: Attribute<int>
  {
    public IntAttribute(int payload) : base(payload)
    {
    }

    public static IntAttribute FromString(string encodedPayload) {
      if (int.TryParse(encodedPayload, out int parsePay)) {
        return new IntAttribute(parsePay);
      }
      return null;
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

