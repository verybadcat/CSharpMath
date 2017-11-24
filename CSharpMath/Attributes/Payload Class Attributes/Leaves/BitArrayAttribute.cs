using System;
using System.Collections;

namespace CSharpMath
{
  public class BitArrayAttribute: Attribute<BitArray>
  {
    public BitArrayAttribute(BitArray payload) : base(payload)
    {
      
    }
    public BitArrayAttribute() : this(new BitArray(0))
    {
      
    }
    public override BitArray NarrowClonePayload() {
      if (this.Payload == null) {
        return null;
      }
      return new BitArray(this.Payload);
    }
    /// <summary>Actually defaults to new BitArray(0). Creates if absent</summary>
    public override BitArray PayloadBitArray(BitArray defaultValue = null) {
      if (defaultValue == null) {
        defaultValue = new BitArray(0);
      }
      BitArray r = this.Payload;
      if (r == null) {
        r = defaultValue;
        this.Payload = r;
      }
      return r;
    }
    public override GenericAttribute CloneAttribute() {
      return new BitArrayAttribute(new BitArray(this.Payload));
    }
    public override bool PayloadsAreEqual(BitArray payload1, BitArray payload2) {
      bool r = false;
      int length = payload1.Length;
      if (length == payload2.Length) {
        r = true;
        for (int i = 0; i < length; i++) {
          if (payload1[i] != payload2[i]) {
            r = false;
            break;
          }
        }
      }
      return r;
    }
  }
}
