using System;

namespace CSharpMath
{
  /// <summary>Converts objects of class T to GenericAttributes and back.
  /// It is expected that taking a T, making an attribute, and converting
  /// back will return the original T.</summary>
  public interface IAttributeConverter<T>: IResultingAttributeVisitor<T>
  {
    GenericAttribute ToAttribute(T t);
  }
  public static class IAttributeConverterAdditions {
    public static T FromAttribute<T>(this IAttributeConverter<T> converter, GenericAttribute attribute) {
      T r = attribute.Accept(converter);
      return r;
    }
  }
}

