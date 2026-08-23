namespace CSharpMath.Atom.Atoms {
  /// <summary>A brace group `{…}` in math mode — an Ord subformula. Script-capable;
  /// spaced as Ordinary. The grouped content lives in <see cref="InnerList"/>, and the
  /// sub/superscript fields drive scripting of the whole group.</summary>
  public sealed class Group : MathAtom, IMathListContainer {
    public MathList InnerList { get; set; } = new MathList();
    public override bool ScriptsAllowed => true;
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
      new[] { InnerList };
    public new Group Clone(bool finalize) => (Group)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Group {
      InnerList = InnerList.Clone(finalize)
    };
    /// <summary>Offsets every atom range of the inner list so group contents carry
    /// global indices starting at <paramref name="startIndex"/> (finalization only).</summary>
    internal void OffsetInnerRanges(int startIndex) {
      foreach (var atom in InnerList)
        if (atom.IndexRange != Range.Zero && atom.IndexRange.Location < startIndex)
          atom.IndexRange = new Range(atom.IndexRange.Location + startIndex, atom.IndexRange.Length);
      // Recurse into nested containers.
      foreach (var atom in InnerList)
        if (atom is Group nested) nested.OffsetInnerRanges(startIndex);
    }
    public override string DebugString =>
      new System.Text.StringBuilder()
        .AppendInBracesOrEmptyBraces(InnerList.DebugString)
        .AppendDebugStringOfScripts(this).ToString();
  }
}

