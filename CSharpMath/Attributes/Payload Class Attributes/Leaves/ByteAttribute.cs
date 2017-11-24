using System;
namespace CSharpMath
{
  public class ByteAttribute: Attribute<byte>
  {
    public ByteAttribute(byte payload): base(payload)
    {
    }
    public ByteAttribute(ByteAttribute other) {
      this.Payload = other.Payload;
    }
    public override GenericAttribute CloneAttribute() {
      return new ByteAttribute(this);
    }
    public override byte PayloadByte(byte defaultValue = 255)
    {
      return this.Payload;
    }
    public override int? PayloadIntQ {
      get {
        return this.Payload;
      }
    }
    public override int PayloadInt(int defaultValue)
    {
      return this.Payload;
    }
  }
}
