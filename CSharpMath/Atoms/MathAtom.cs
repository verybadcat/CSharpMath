using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public abstract class MathAtom : IScripts, IMathObject {
    public virtual string DebugString =>
      new StringBuilder(Nucleus).AppendDebugStringOfScripts(this).ToString();
    public MathAtomType AtomType { get; set; }
    public string Nucleus { get; set; }
    private MathList? _Superscript;
    public MathList? Superscript {
      get => _Superscript;
      set =>
        _Superscript = ScriptsAllowed || value == null ? value :
          throw new ArgumentException("Scripts are not allowed in atom type " + AtomType.ToText());
    }
    private MathList? _Subscript;
    public MathList? Subscript {
      get => _Subscript;
      set =>
        _Subscript = ScriptsAllowed || value == null ? value :
          throw new ArgumentException("Scripts are not allowed in atom type " + AtomType.ToText());
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
    public MathAtom Clone(bool finalize) {
      var newAtom = CloneInside(finalize);
      newAtom.AtomType = AtomType;
      newAtom.Nucleus = Nucleus;
      if (FusedAtoms != null)
        newAtom.FusedAtoms = new List<MathAtom>(FusedAtoms);
      newAtom.Superscript = Superscript?.Clone(finalize);
      newAtom.Subscript = Subscript?.Clone(finalize);
      newAtom.IndexRange = IndexRange;
      newAtom.FontStyle = FontStyle;
      return newAtom;
    }
    public MathAtom(MathAtomType type, string nucleus = "") =>
      (AtomType, Nucleus) = (type, nucleus);
    public void Fuse(MathAtom otherAtom) {
      if (Subscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a subscript " + DebugString);
      }
      if (Superscript != null) {
        throw new InvalidOperationException("Cannot fuse into an atom with a superscript " + DebugString);
      }
      if (otherAtom.AtomType != AtomType) {
        throw new InvalidOperationException("Cannot fuse atoms with different types");
      }
      FusedAtoms ??= new List<MathAtom> {
        Clone(false)
      };
      if (otherAtom.FusedAtoms != null) {
        FusedAtoms.AddRange(otherAtom.FusedAtoms);
      } else {
        FusedAtoms.Add(otherAtom);
      }
      Nucleus += otherAtom.Nucleus;
      IndexRange = new Range(IndexRange.Location, IndexRange.Length + otherAtom.IndexRange.Length);
      Subscript = otherAtom.Subscript;
      Superscript = otherAtom.Superscript;
    }

    public override string ToString() =>
      AtomType.ToText() + " " + DebugString;
    public bool EqualsAtom(MathAtom? otherAtom) =>
      otherAtom != null &&
      Nucleus == otherAtom.Nucleus &&
      AtomType == otherAtom.AtomType &&
      Superscript.NullCheckingEquals(otherAtom.Superscript) &&
      Subscript.NullCheckingEquals(otherAtom.Subscript) &&
      //IndexRange == otherAtom.IndexRange &&
      //FontStyle == otherAtom.FontStyle &&
      otherAtom.GetType() == this.GetType();
    public override bool Equals(object obj) => EqualsAtom(obj as MathAtom);
    public override int GetHashCode() => unchecked(
        AtomType.GetHashCode()
        + 3 * (Superscript?.GetHashCode() ?? 0)
        + 5 * (Subscript?.GetHashCode() ?? 0)
        //+ 7 * IndexRange.GetHashCode()
        //+ 13 * FontStyle.GetHashCode()
        + 307 * Nucleus.GetHashCode());
  }
}
