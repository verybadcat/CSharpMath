using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms.Atom {
  public class Space : MathAtom {
    private readonly Structures.Space _space;
    public float Length => _space.Length;
    public bool IsMu => _space.IsMu;
    public Space(Structures.Space space) : base(MathAtomType.Space, string.Empty) =>
      _space = space;
    public override bool ScriptsAllowed => false;
    public new Space Clone(bool finalize) => (Space)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Space(_space);
    public float ActualLength<TFont, TGlyph>(FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.IFont<TGlyph> => _space.ActualLength(mathTable, font);
    public override string DebugString => " ";
    public override bool Equals(object obj) => obj is Space s && EqualsSpace(s);
    public bool EqualsSpace(Space otherSpace) =>
      EqualsAtom(otherSpace) && Length == otherSpace.Length && IsMu == otherSpace.IsMu;
    public override int GetHashCode() => unchecked(base.GetHashCode() + 3 * _space.GetHashCode());
  }
}