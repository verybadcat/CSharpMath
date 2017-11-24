using System;
using System.Drawing;

namespace CSharpMath
{
  public class ColorAttribute: Attribute<Color>
  {
    public ColorAttribute(Color payload) : base(payload)
    {
    }
    public override Color? PayloadColor(Color? defaultValue = null) {
      Color r = this.Payload;
      return r;
    }
    public override GenericAttribute CloneAttribute() {
      return new ColorAttribute(this.Payload);
    }
  }
}

