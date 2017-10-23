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

    public Range IndexRange { get; set; }

    public List<IMathAtom> FusedAtoms { get; set; }

    public bool ScriptsAllowed() => ItemType < CSharpMath.MathItemType.Boundary;

    public MathAtom(MathItemType type, string nucleus) {
      ItemType = type;
      Nucleus = nucleus;
    }
    public MathAtom(MathAtom cloneMe, bool finalize) {
      ItemType = cloneMe.ItemType;
      Nucleus = cloneMe.Nucleus;
      Superscript = AtomCloner.Clone(cloneMe.Superscript, finalize);
      Subscript = AtomCloner.Clone(cloneMe.Subscript, finalize);
      IndexRange = cloneMe.IndexRange;
      FontStyle = cloneMe.FontStyle;
    }

    public void Fuse(IMathAtom otherAtom) {
      if (Subscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a subscript " + StringValue);
      }
      if (Superscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a superscript " + StringValue);
      }
      if (otherAtom.ItemType != ItemType) {
        throw new InvalidOperationException("Cannot fuse atoms with different types");
      }
      if (FusedAtoms == null) {
        FusedAtoms = new List<IMathAtom> {
          AtomCloner.Clone(this, false)
        };
      }
      if (otherAtom.FusedAtoms!=null) {
        FusedAtoms.AddRange(otherAtom.FusedAtoms);
      } else {
        FusedAtoms.Add(otherAtom);
      }
      Nucleus += otherAtom.Nucleus;
      IndexRange = new Range(IndexRange.Location, IndexRange.Length + otherAtom.IndexRange.Length);
      Subscript = otherAtom.Subscript;
      Superscript = otherAtom.Superscript;
    }

    public virtual T Accept<T, THelper>(IMathAtomVisitor<T, THelper> visitor, THelper helper)
      => visitor.Visit(this, helper);
  }
}
