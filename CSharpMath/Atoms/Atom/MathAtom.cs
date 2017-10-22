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

    public List<IMathAtom> FusedAtoms => throw new NotImplementedException();

    public bool ScriptsAllowed() => throw new NotImplementedException();
    public MathAtom(MathAtom atom, bool finalize) {
      ItemType = atom.ItemType;
      Nucleus = atom.Nucleus;
      Superscript = AtomCloner.Clone(atom.Superscript, finalize);
    }
    public virtual T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
      => visitor.Visit(this, helper);
  }
}
