using System;
using CSharpMath.EncodingAttributeHandlingsSpecific;

namespace CSharpMath
{
  public static class EncodingAttributeHandlings
  {
    public static EncodingAttributeHandling All {get;} = new EncodingAttributeHandlingAll();
    public static EncodingAttributeHandling None {get;} = new EncodingAttributeHandlingNone();
    public static EncodingAttributeHandling TypicalExpression {get;} = new EncodingAttributeHandlingAllButCursorPosition();
    public static EncodingAttributeHandling Wrapper {get;} = new EncodingAttributeHandlingWrapper();
    public static EncodingAttributeHandling WithoutDisplayString(EncodingAttributeHandling inner) {
      return new EncodingAttributeHandlingWithoutDisplayString(inner);
    }
  }
}
