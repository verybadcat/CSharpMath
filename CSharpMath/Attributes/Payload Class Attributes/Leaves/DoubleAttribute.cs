using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using CSharpMath.Maths;
using numericType = System.Double;

namespace CSharpMath
{
  public class DoubleAttribute: Attribute<numericType>
  {
    public DoubleAttribute(numericType value)
      : base(value)
    {
    }
    public override bool PayloadsAreEqual(numericType payload1, numericType payload2) {
      return (payload1 == payload2 || double.IsNaN(payload1) && double.IsNaN(payload2));
    }
    public override numericType PayloadDouble(numericType defaultValue = numericType.NaN) {
      numericType r = this.Payload;
      return r;
    }
    public override float PayloadFloat(float defaultValue) {
      double r = this.PayloadDouble();
      if (r > float.MaxValue) {
        r = float.MaxValue;
      }
      if (r < float.MinValue) {
        r = float.MinValue;
      }
      return (float)r;
    }
    public DoubleAttribute(string s)
    {
      numericType value = DoubleAdditions.TryParse(s);
      this.Payload = value;
    }
    public override bool PayloadIsUndefined() {
      return numericType.IsNaN(this.Payload);
    }
    public override GenericAttribute CloneAttribute() {
      return new DoubleAttribute(this.Payload);
    }
  }
}
