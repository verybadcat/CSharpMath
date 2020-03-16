using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atom
 {
  public abstract class MathAtom : IMathObject, IEquatable<MathAtom> {
    public string TypeName {
      get {
        // Insert a space before every capital letter other than the first one.
        var itemString = GetType().Name;
        var chars = new StringBuilder(itemString);
        for (int i = itemString.Length - 1; i > 0; i--)
          if (char.IsUpper(chars[i]))
            chars.Insert(i, ' ');
        return chars.ToString();
      }
    }
    public virtual string DebugString =>
      new StringBuilder(Nucleus).AppendDebugStringOfScripts(this).ToString();
    public string Nucleus { get; set; }
    private MathList? _Superscript;
    public MathList? Superscript {
      get => _Superscript;
      set =>
        _Superscript = ScriptsAllowed || value == null ? value :
          throw new ArgumentException("Scripts are not allowed in atom type " + TypeName);
    }
    private MathList? _Subscript;
    public MathList? Subscript {
      get => _Subscript;
      set =>
        _Subscript = ScriptsAllowed || value == null ? value :
          throw new ArgumentException("Scripts are not allowed in atom type " + TypeName);
    }
    public FontStyle FontStyle { get; set; }

    public Range IndexRange { get; set; }

    /// <summary>
    /// If this atom was formed by fusion of multiple atoms, then this stores the list
    /// of atoms that were fused to create this one. This is used in the finalizing
    /// and preprocessing steps.
    /// </summary>
    public List<MathAtom>? FusedAtoms { get; set; }

    /// <summary>
    /// Whether or not the atom allows superscripts and subscripts.
    /// </summary>
    public abstract bool ScriptsAllowed { get; }
    protected abstract MathAtom CloneInside(bool finalize);
    protected TAtom ApplyCommonPropertiesOn<TAtom>(bool finalize, TAtom newAtom)
      where TAtom : MathAtom {
      if (string.IsNullOrEmpty(newAtom.Nucleus))
        // newAtom.Nucleus may have already been initialized by newAtom's constructor
        newAtom.Nucleus = Nucleus;
      if (FusedAtoms != null)
        newAtom.FusedAtoms = new List<MathAtom>(FusedAtoms);
      newAtom.Superscript = Superscript?.Clone(finalize);
      newAtom.Subscript = Subscript?.Clone(finalize);
      newAtom.IndexRange = IndexRange;
      newAtom.FontStyle = FontStyle;
      return newAtom;
    }
    public MathAtom Clone(bool finalize) =>
      ApplyCommonPropertiesOn(finalize, CloneInside(finalize));
    public MathAtom(string nucleus = "") => Nucleus = nucleus;
    public void Fuse(MathAtom otherAtom) {
      if (Subscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a subscript");
      }
      if (Superscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a superscript");
      }
      if (otherAtom.GetType() != GetType()) {
        throw new InvalidOperationException("Cannot fuse atoms with different types");
      }
      FusedAtoms ??= new List<MathAtom> { Clone(false) };
      if (otherAtom.FusedAtoms != null) {
        FusedAtoms.AddRange(otherAtom.FusedAtoms);
      } else {
        FusedAtoms.Add(otherAtom);
      }
      Nucleus += otherAtom.Nucleus;
      IndexRange = new Range(IndexRange.Location,
        IndexRange.Length + otherAtom.IndexRange.Length);
      Subscript = otherAtom.Subscript;
      Superscript = otherAtom.Superscript;
    }

    public override string ToString() =>
      TypeName + " " + DebugString;
    public bool EqualsAtom(MathAtom otherAtom) =>
      Nucleus == otherAtom.Nucleus &&
      GetType() == otherAtom.GetType() &&
      //IndexRange == otherAtom.IndexRange &&
      //FontStyle == otherAtom.FontStyle &&
      Superscript.NullCheckingStructuralEquality(otherAtom.Superscript) &&
      Subscript.NullCheckingStructuralEquality(otherAtom.Subscript);
    public override bool Equals(object obj) => obj is MathAtom a ? EqualsAtom(a) : false;
    bool IEquatable<MathAtom>.Equals(MathAtom otherAtom) => EqualsAtom(otherAtom);
    public override int GetHashCode() => unchecked(
        GetType().GetHashCode()
        + 3 * (Superscript?.GetHashCode() ?? 0)
        + 5 * (Subscript?.GetHashCode() ?? 0)
        //+ 7 * IndexRange.GetHashCode()
        //+ 13 * FontStyle.GetHashCode()
        + 307 * Nucleus.GetHashCode());
  }
}