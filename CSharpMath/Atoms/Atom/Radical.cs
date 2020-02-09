using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;

namespace CSharpMath.Atoms.Atom {
  public class Radical: MathAtom {
    public MathList? Degree { get; set; }
    /// <summary>Whatever is under the square root sign</summary>
    public MathList Radicand { get; set; }
    public Radical(MathList? degree, MathList radicand)
      : base(MathAtomType.Radical, string.Empty) =>
      (Degree, Radicand) = (degree, radicand);
    public new Radical Clone(bool finalize) => (Radical)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Radical(Degree?.Clone(finalize), Radicand.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public override string DebugString =>
      new StringBuilder(@"\Sqrt")
      .AppendInSquareBrackets(Degree?.DebugString, NullHandling.EmptyString)
      .AppendInBraces(Radicand.DebugString, NullHandling.LiteralNull)
      .AppendDebugStringOfScripts(this).ToString();
  }
}
