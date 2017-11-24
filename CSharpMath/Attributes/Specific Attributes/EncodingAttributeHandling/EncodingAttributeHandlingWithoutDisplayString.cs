using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.EncodingAttributeHandlingsSpecific
{
  class EncodingAttributeHandlingWithoutDisplayString: EncodingAttributeHandling
  {
    public EncodingAttributeHandling InnerHandling { get; set; }
    public EncodingAttributeHandlingWithoutDisplayString(EncodingAttributeHandling inner) {
      this.InnerHandling = inner;
    }
    public override Boolean ShouldEncodeAttribute(String key, GenericAttribute attribute) {
      if (key == AttributeKeys.DISPLAY_STRING_KEY) {
        return false;
      }
      EncodingAttributeHandling inner = this.InnerHandling;
      return inner.ShouldEncodeAttribute(key, attribute);
    }
  }
}
