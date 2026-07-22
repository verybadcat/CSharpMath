namespace CSharpMath.Atom.Atoms;
/// <summary>
/// Abstract name of under annotations implementation \underbrace, \underbracket etc..
/// </summary>
public sealed class UnderAnnotation : MathAtom, IMathListContainer {
  public UnderAnnotation(string value, MathList? innerList = null, MathList? underList = null) : base(value) {
    InnerList = innerList ?? new MathList();
    UnderList = underList ?? new MathList();
  }

  public MathList InnerList { get; } = new MathList();
  public MathList? UnderList { get; set; } = new MathList();

  System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
    [InnerList ?? new MathList()];
  public new UnderAnnotation Clone(bool finalize) => (UnderAnnotation)base.Clone(finalize);
  protected override MathAtom CloneInside(bool finalize) =>
    new UnderAnnotation(Nucleus, InnerList?.Clone(finalize), UnderList?.Clone(finalize));
  public override bool ScriptsAllowed => true;
  //depending on nucleus, DebugString will change to \underbrace , \underbracket etc..
  public override string DebugString =>
    new System.Text.StringBuilder(@"\underbrace")
    .AppendInBracesOrLiteralNull(InnerList?.DebugString)
    .Append(UnderList is { Count: > 0 } ? $"_{{{UnderList?.DebugString}}}" : string.Empty)
    .ToString();
  public bool EqualsUnderAnnotation(UnderAnnotation other) =>
    EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other.InnerList)
    && UnderList.NullCheckingStructuralEquality(other.UnderList);
}