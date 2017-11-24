using System;
using CSharpMath.Debugging;

namespace CSharpMath
{
  public abstract class ComposingAttributeConverter<T, TMid>: ComposingAttributeVisitor<T, TMid>, IAttributeConverter<T>
  {
    public AttributeConverter<TMid> InnerConverter {get;set;}  // actually a dup of InnerVisitor, other than the type.
    public abstract TMid ToMid(T t);

    public GenericAttribute ToAttribute(T t) {
      TMid mid = this.ToMid(t);
      var inner = this.InnerConverter;
      GenericAttribute r = inner.ToAttribute(mid);
      return r;
    }

    public ComposingAttributeConverter(AttributeConverter<TMid> midConverter): base(midConverter)
    {
      this.InnerConverter = midConverter;
    }



  }
}

