using System;
using CSharpMath.Debugging;

namespace CSharpMath
{
  public abstract class ComposingAttributeVisitor<T, TMid>: AttributeVisitor<T>
  {
    public AttributeVisitor<TMid> InnerVisitor { get; set;}
    public ComposingAttributeVisitor(AttributeVisitor<TMid> innerVisitor)
    {
      this.InnerVisitor = innerVisitor;
    }
    public abstract T FromMid(TMid mid);
    public override T Visit(ArrayAttribute attribute) {
      var inner = this.InnerVisitor;
      TMid mid = inner.Visit(attribute);
      T r = this.FromMid(mid);
      return r;
    }

    public override T Visit(DictionaryAttribute attribute) {
      var inner = this.InnerVisitor;
      TMid mid = inner.Visit(attribute);
      T r = this.FromMid(mid);
      return r;
    }

    public override T Visit(StringAttribute attribute) {
      var inner = this.InnerVisitor;
      TMid mid = inner.Visit(attribute);
      T r = this.FromMid(mid);
      return r;
    }

    public override T VisitGeneric(GenericAttribute attribute) {
      var inner = this.InnerVisitor;
      TMid mid = inner.VisitGeneric(attribute);
      T r = this.FromMid(mid);
      return r;
    }

  }
}

