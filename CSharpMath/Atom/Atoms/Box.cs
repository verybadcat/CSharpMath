namespace CSharpMath.Atom.Atoms {
  /// <summary>Horizontal alignment of a box's child relative to the box origin
  /// (drives the lap draw offset).</summary>
  public enum BoxHAlign { Left, Center, Right }
  /// <summary>Overlay strike drawn across an otherwise-unchanged box:
  /// the cancel/sout family.</summary>
  public enum StrikeStyle { None, Forward, Backward, Cross, Horizontal }

  /// <summary>A box atom: the phantom/smash/lap family plus the cancel family.
  /// The content lives in <see cref="InnerList"/>; the flags below select the variant.</summary>
  public sealed class Box : MathAtom, IMathListContainer {
    public MathList InnerList { get; set; } = new MathList();
    /// <summary>Report the child's width (true) or zero width (false).</summary>
    public bool KeepWidth { get; set; } = true;
    /// <summary>Report the child's ascent (true) or zero ascent (false).</summary>
    public bool KeepHeight { get; set; } = true;
    /// <summary>Report the child's descent (true) or zero descent (false).</summary>
    public bool KeepDepth { get; set; } = true;
    /// <summary>Draw the measured child (true) or suppress drawing entirely (false, phantom).</summary>
    public bool DrawChild { get; set; } = true;
    /// <summary>Horizontal draw offset applied when KeepWidth == false (laps).</summary>
    public BoxHAlign HAlign { get; set; }
    /// <summary>Overlay strike drawn across the box; None (default) = no strike.</summary>
    public StrikeStyle StrikeStyle { get; set; }
    public override bool ScriptsAllowed => true;
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public new Box Clone(bool finalize) => (Box)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Box {
      InnerList = InnerList.Clone(finalize),
      KeepWidth = KeepWidth,
      KeepHeight = KeepHeight,
      KeepDepth = KeepDepth,
      DrawChild = DrawChild,
      HAlign = HAlign,
      StrikeStyle = StrikeStyle
    };
    public override string DebugString =>
      new System.Text.StringBuilder(@"\box")
        .AppendInBracesOrEmptyBraces(InnerList.DebugString)
        .AppendDebugStringOfScripts(this).ToString();
  }
}
