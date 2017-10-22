using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathAtom : IMathAtom {

    public virtual string StringValue {
      get {
        StringBuilder builder = new StringBuilder(Nucleus);
        builder.AppendInBraces(Superscript.StringValue, NullHandling.EmptyString);
        builder.AppendInBraces(Subscript.StringValue, NullHandling.EmptyString);
        return builder.ToString();
      }
    }
    public MathItemType ItemType { get; set; }
    public string Nucleus { get; set; }
    public IMathList Superscript { get; set; }
    public IMathList Subscript { get; set; }
    public FontStyle FontStyle { get; set; }

    public Range IndexRange { get; protected set; }

    public List<IMathAtom> FusedAtoms => throw new NotImplementedException();

    public bool ScriptsAllowed() => ItemType < CSharpMath.MathItemType.Boundary;
    public MathAtom(MathItemType type, string nucleus) {
      ItemType = type;
      Nucleus = nucleus;
    }
    public MathAtom(MathAtom cloneMe, bool finalize) {
      ItemType = cloneMe.ItemType;
      Nucleus = cloneMe.Nucleus;
      Superscript = AtomCloner.Instance.Clone(cloneMe.Superscript, finalize);
      Subscript = AtomCloner.Instance.Clone(cloneMe.Subscript, finalize);
      IndexRange = cloneMe.IndexRange;
      FontStyle = cloneMe.FontStyle;
    }
    public virtual T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
      => visitor.Visit(this, helper);
  }
}
