using System;
namespace CSharpMath
{
  public static class AttributeCollections
  {
    public static AttributeCollection FromString (string text) {
      return new AttributeCollection(text);
    }
  }
}
