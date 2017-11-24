using System;

namespace CSharpMath
{
  public interface IResultingAttributeVisitor<TResult>
  {
    TResult VisitGeneric(GenericAttribute attribute);
    TResult Visit(DictionaryAttribute attribute);
    TResult Visit(ArrayAttribute attribute);
    TResult Visit(StringAttribute attribute);
  }
}

