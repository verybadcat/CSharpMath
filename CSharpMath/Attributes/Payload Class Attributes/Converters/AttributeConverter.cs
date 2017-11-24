using System;

namespace CSharpMath
{
  public abstract class AttributeConverter<T>: AttributeVisitor<T>, IAttributeConverter<T>
  {
    public AttributeConverter()
    {
    }

    public abstract GenericAttribute ToAttribute(T t);
    public virtual void Setup(Func<T> makeTarget, bool force) {

    }
  }

  public static class AttributeConverterAdditions
  {
    public static void Setup<T>(this AttributeConverter<T> converter, T target, bool force) {
      converter.Setup(() => target, force);
    }
  }

}

