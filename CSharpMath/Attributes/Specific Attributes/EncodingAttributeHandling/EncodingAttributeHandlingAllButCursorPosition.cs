using System;

namespace CSharpMath.EncodingAttributeHandlingsSpecific
{
  internal class EncodingAttributeHandlingAllButCursorPosition: EncodingAttributeHandling
  {
    public override bool ShouldEncodeAttribute(string key, GenericAttribute attribute) {
      switch (key) {
        case AttributeKeys.CURSOR_LOGICAL_INDEX_KEY:
        case AttributeKeys.ISSTATIC_KEY:
          return false;
        default:
          return true;
      }
    }
  }
}

