using System;

namespace CSharpMath.EncodingAttributeHandlingsSpecific
{
  internal class EncodingAttributeHandlingAll: EncodingAttributeHandling
  {
    public override bool ShouldEncodeAttribute(string key, GenericAttribute attribute) {
      return true;
    }
  }
}

