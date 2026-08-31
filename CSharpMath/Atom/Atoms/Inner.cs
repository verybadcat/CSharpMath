using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atom.Atoms {
  /// <summary>An inner atom, i.e. embedded math list</summary>
  public sealed class Inner : MathAtom, IMathListContainer {
    public Inner(Boundary left, MathList innerList, Boundary right)
      : this(left, new[] { innerList }, Array.Empty<Boundary>(), right) { }
    public Inner(Boundary left, IReadOnlyList<MathList> segments,
      IReadOnlyList<Boundary> middleBoundaries, Boundary right) {
      if (segments is null) throw new ArgumentNullException(nameof(segments));
      if (middleBoundaries is null) throw new ArgumentNullException(nameof(middleBoundaries));
      if (segments.Count != middleBoundaries.Count + 1)
        throw new ArgumentException("An inner atom must have one more segment than middle delimiters.");
      (LeftBoundary, Segments, MiddleBoundaries, RightBoundary) =
        (left, Array.AsReadOnly(segments.ToArray()),
          Array.AsReadOnly(middleBoundaries.ToArray()), right);
    }
    public MathList InnerList => Segments[0];
    public IReadOnlyList<MathList> Segments { get; }
    public IReadOnlyList<Boundary> MiddleBoundaries { get; }
    public Boundary LeftBoundary { get; }
    public Boundary RightBoundary { get; }
    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists => Segments;
    public override bool ScriptsAllowed => true;
    public new Inner Clone(bool finalize) => (Inner)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Inner(LeftBoundary, Segments.Select(s => s.Clone(finalize)).ToArray(),
        MiddleBoundaries, RightBoundary);
    public bool EqualsInner(Inner otherInner) =>
      EqualsAtom(otherInner)
      && Segments.Count == otherInner.Segments.Count
      && Segments.Zip(otherInner.Segments, (a, b) => a.NullCheckingStructuralEquality(b)).All(x => x)
      && MiddleBoundaries.SequenceEqual(otherInner.MiddleBoundaries)
      && LeftBoundary.NullCheckingStructuralEquality(otherInner.LeftBoundary)
      && RightBoundary.NullCheckingStructuralEquality(otherInner.RightBoundary);
    public override bool Equals(object obj) => obj is Inner i ? EqualsInner(i) : false;
    public override int GetHashCode() =>
      (base.GetHashCode(), string.Join("\u001f", Segments.Select(s => s.DebugString)),
        string.Join("\u001f", MiddleBoundaries.Select(b => b.Nucleus)),
        LeftBoundary, RightBoundary).GetHashCode();
    public override string DebugString => MiddleBoundaries.Count == 0
      ? new StringBuilder(@"\inner").AppendInBracesOrEmptyBraces(LeftBoundary.Nucleus)
        .AppendInBracesOrLiteralNull(InnerList.DebugString)
        .AppendInBracesOrEmptyBraces(RightBoundary.Nucleus)
        .AppendDebugStringOfScripts(this).ToString()
      : new StringBuilder(@"\inner").AppendInBracesOrEmptyBraces(LeftBoundary.Nucleus)
        .AppendInBracesOrLiteralNull(string.Join("|", Segments.Select(s => s.DebugString)))
        .AppendInBracesOrLiteralNull(string.Join(",", MiddleBoundaries.Select(b => b.Nucleus)))
        .AppendInBracesOrEmptyBraces(RightBoundary.Nucleus)
        .AppendDebugStringOfScripts(this).ToString();
  }
}
