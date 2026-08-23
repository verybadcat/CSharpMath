namespace CSharpMath.Atom.Atoms {
  /// <summary>Describes how one row (over or under) of a <see cref="Stack"/> is produced.</summary>
  public abstract class StackConstruction {
    /// <summary>A single stretchy cap glyph (e.g. → for \overrightarrow, ⏞ for \overbrace).</summary>
    public sealed class Extensible : StackConstruction {
      public string Glyph { get; }
      public Extensible(string glyph) => Glyph = glyph;
    }
    /// <summary>A math list typeset at a specified style (for \overset / \underset / …).</summary>
    public sealed class MathListRow : StackConstruction {
      public Atom.MathList List { get; }
      public LineStyle Style { get; }
      public bool Cramped { get; }
      public MathListRow(Atom.MathList list, LineStyle style, bool cramped) =>
        (List, Style, Cramped) = (list, style, cramped);
    }
    internal StackConstruction() { }

    public StackConstruction Clone(bool finalize) {
      if (this is Extensible e) return new Extensible(e.Glyph);
      if (this is MathListRow m) return new MathListRow(m.List.Clone(finalize), m.Style, m.Cramped);
      throw new InvalidCodePathException("Unknown stack construction kind");
    }
  }

  /// <summary>A generic over/under stack atom: \overrightarrow, \overbrace,
  /// \underset and relatives. The display class controls inter-element spacing
  /// (default Ordinary; \stackrel forces Relation, \stackbin BinaryOperator).</summary>
  public sealed class Stack : MathAtom, IMathListContainer {
    public MathList InnerList { get; set; } = new MathList();
    public StackConstruction? Over { get; set; }
    public StackConstruction? Under { get; set; }
    public System.Type DisplayClassType { get; set; } = typeof(Ordinary);
    public override bool ScriptsAllowed => true;
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public new Stack Clone(bool finalize) => (Stack)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Stack {
      InnerList = InnerList.Clone(finalize),
      Over = Over?.Clone(finalize),
      Under = Under?.Clone(finalize),
      DisplayClassType = DisplayClassType
    };
    public override string DebugString =>
      new System.Text.StringBuilder(@"\stack")
        .AppendInBracesOrEmptyBraces(InnerList.DebugString)
        .AppendDebugStringOfScripts(this).ToString();
  }
}
