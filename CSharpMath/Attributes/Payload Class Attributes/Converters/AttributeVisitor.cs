using System;

namespace CSharpMath
{
  public class AttributeVisitor<T>: IResultingAttributeVisitor<T>
  {
    public AttributeVisitor()
    {
    }

    #region IAttributeVisitor implementation

    public virtual T VisitGeneric(GenericAttribute attribute) {
      return default(T);
    }

    public virtual T Visit(DictionaryAttribute attribute) {
      return default(T);
    }

    public virtual T Visit(ArrayAttribute attribute) {
      return default(T);
    }

    public virtual T Visit(StringAttribute attribute) {
      return default(T);
    }

    #endregion
  }
}

