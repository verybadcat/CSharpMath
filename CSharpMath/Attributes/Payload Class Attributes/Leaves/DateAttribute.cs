using System;
namespace CSharpMath
{
  public class DateAttribute: Attribute<DateTime>
  {
    public DateAttribute()
    {
    }


    public DateAttribute(DateTime payload) : base(payload){}

    public override DateTime PayloadDateTime(DateTime defaultValue) {
      return this.Payload;
    }

    public DateAttribute(DateAttribute other): this(other.Payload) {}

    public override GenericAttribute CloneAttribute()
    {
      return new DateAttribute(this);
    }
  }
}

