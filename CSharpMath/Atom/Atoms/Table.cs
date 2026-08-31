using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Atom.Atoms {
  internal sealed class MulticolumnAtom : MathAtom {
    public MulticolumnAtom(int span, ColumnAlignment alignment, MathList content, string specification = "") : base(string.Empty) {
      if (span < 1) throw new System.ArgumentOutOfRangeException(nameof(span));
      Span = span; Alignment = alignment; Specification = specification; Content = content;
    }
    public int Span { get; }
    public ColumnAlignment Alignment { get; }
    public string Specification { get; }
    public MathList Content { get; }
    public override bool ScriptsAllowed => false;
    protected override MathAtom CloneInside(bool finalize) => new MulticolumnAtom(Span, Alignment, Content.Clone(finalize), Specification);
    public override string DebugString => $@"\multicolumn{{{Span}}}{{{Specification}}}{{{Content.DebugString}}}";
    public override bool Equals(object obj) => obj is MulticolumnAtom m && Span == m.Span && Alignment == m.Alignment && Specification == m.Specification && Content.Equals(m.Content);
    public override int GetHashCode() => (Span, Alignment, Specification, Content).GetHashCode();
  }
  ///<summary>A table. Not part of TeX.</summary>
  public sealed class Table : MathAtom, IMathListContainer {
    public Table(string? environment, List<List<MathList>>? cells = null) : base(string.Empty) =>
      (Environment, Cells) = (environment, cells ?? new List<List<MathList>>());
    public Table() : this(null) { }
    /// <summary>Deep copy, finalized or not.</summary>
    public new Table Clone(bool finalize) => (Table)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Table(Environment) {
      InterColumnSpacing = InterColumnSpacing,
      InterRowAdditionalSpacing = InterRowAdditionalSpacing,
      Environment = Environment,
      CellStyle = CellStyle,
      VerticalLines = VerticalLines.ToList(),
      HorizontalLines = HorizontalLines.ToList(),
      ColumnSpans = ColumnSpans.Select(row => row.ToList()).ToList(),
      SpanAlignments = SpanAlignments.Select(row => row.ToList()).ToList(),
      SpanSpecifications = SpanSpecifications.Select(row => row.ToList()).ToList(),
      Alignments = Alignments.ToList(),
      Cells = new List<List<MathList>>(Cells.Select(list =>
        new List<MathList>(list.Select(sublist => sublist.Clone(finalize)))))
    };
    public override bool ScriptsAllowed => false;
    public List<ColumnAlignment> Alignments { get; private set; } =
      new List<ColumnAlignment>();
    /// <summary>Cells[i][j] == cell at ith row and jth column</summary>
    public List<List<MathList>> Cells { get; private set; }
    IEnumerable<MathList> IMathListContainer.InnerLists => Cells.SelectMany(row => row);
    /// <summary>Space between columns in mu units.</summary>
    public float InterColumnSpacing { get; set; }
    /// <summary>
    /// Additional spacing between rows in jots (one jot is 0.3 times font size).
    /// </summary>
    public float InterRowAdditionalSpacing { get; set; }
    /// <summary>The line style every cell of this table renders in, or null to
    /// inherit the surrounding style (aligned/gather/eqnarray/alignedat).</summary>
    public LineStyle? CellStyle { get; set; }
    /// <summary>The number of `|` vertical rules at each column boundary (array
    /// environment). Length NColumns+1: index 0 = before column 0 … NColumns = after the
    /// last column. Empty for every non-array environment.</summary>
    public List<int> VerticalLines { get; set; } = new List<int>();
    /// <summary>The number of `\hline` horizontal rules at each row boundary (array
    /// environment). Length NRows+1: index 0 = above row 0 … NRows = below the last
    /// row. Empty for every non-array environment.</summary>
    public List<int> HorizontalLines { get; set; } = new List<int>();
    /// <summary>Cell column spans, parallel to <see cref="Cells"/>; unspecified cells span one column.</summary>
    public List<List<int>> ColumnSpans { get; set; } = new List<List<int>>();
    /// <summary>Explicit alignment for spanning cells, parallel to <see cref="ColumnSpans"/>.</summary>
    public List<List<ColumnAlignment?>> SpanAlignments { get; set; } = new List<List<ColumnAlignment?>>();
    public List<List<string?>> SpanSpecifications { get; set; } = new List<List<string?>>();
    /// <summary>The name of the environment that this table denotes</summary>
    public string? Environment { get; set; }
    /// <summary>Number of rows</summary>
    public int NRows => Cells.Count;
    /// <summary>Number of columns</summary>
    public int NColumns => NRows == 0 ? 0 : Cells.Select((row, i) => row.Select((_, j) => GetColumnSpan(i, j)).Sum()).DefaultIfEmpty(0).Max();
    public void SetCell(MathList list, int iRow, int iColumn) {
      while (Cells.Count <= iRow) Cells.Add(new List<MathList>());
      while (Cells[iRow].Count <= iColumn) Cells[iRow].Add(new MathList());
      Cells[iRow][iColumn] = list;
    }
    public void SetAlignment(ColumnAlignment alignment, int columnIndex) {
      while (Alignments.Count <= columnIndex) Alignments.Add(ColumnAlignment.Center);
      Alignments[columnIndex] = alignment;
    }
    public ColumnAlignment GetAlignment(int columnIndex) =>
      Alignments.Count <= columnIndex ? ColumnAlignment.Center : Alignments[columnIndex];
    public bool EqualsTable(Table otherTable) =>
        EqualsAtom(otherTable) &&
        Cells.SequenceEqual(otherTable.Cells, (c1, c2) => c1.SequenceEqual(c2)) &&
        Alignments.SequenceEqual(otherTable.Alignments) &&
        InterColumnSpacing == otherTable.InterColumnSpacing &&
        InterRowAdditionalSpacing == otherTable.InterRowAdditionalSpacing &&
        CellStyle == otherTable.CellStyle &&
        VerticalLines.SequenceEqual(otherTable.VerticalLines) &&
        HorizontalLines.SequenceEqual(otherTable.HorizontalLines) &&
        ColumnSpans.SequenceEqual(otherTable.ColumnSpans, (a, b) => a.SequenceEqual(b)) &&
        SpanAlignments.SequenceEqual(otherTable.SpanAlignments, (a, b) => a.SequenceEqual(b)) &&
        SpanSpecifications.SequenceEqual(otherTable.SpanSpecifications, (a, b) => a.SequenceEqual(b)) &&
        Environment == otherTable.Environment;
    public override bool Equals(object obj) => obj is Table t ? EqualsTable(t) : false;
    public override int GetHashCode() =>
      (base.GetHashCode(), NestedSequenceHash(Cells), SequenceHash(Alignments),
        InterColumnSpacing, InterRowAdditionalSpacing, CellStyle,
        (SequenceHash(VerticalLines), SequenceHash(HorizontalLines),
        NestedSequenceHash(ColumnSpans), NestedSequenceHash(SpanAlignments),
        NestedSequenceHash(SpanSpecifications), Environment)).GetHashCode();
    private static int NestedSequenceHash(IEnumerable<IEnumerable<MathList>> rows) =>
      SequenceHash(rows.Select(SequenceHash));
    private static int NestedSequenceHash<T>(IEnumerable<IEnumerable<T>> rows) =>
      SequenceHash(rows.Select(SequenceHash));
    private static int SequenceHash<T>(IEnumerable<T> items) {
      unchecked {
        var hash = 17;
        foreach (var item in items) hash = hash * 31 + (item?.GetHashCode() ?? 0);
        return hash;
      }
    }
    public int GetColumnSpan(int row, int column) => row >= 0 && column >= 0 && row < ColumnSpans.Count && column < ColumnSpans[row].Count
      ? ColumnSpans[row][column] : 1;
    public ColumnAlignment? GetSpanAlignment(int row, int column) => row >= 0 && column >= 0 && row < SpanAlignments.Count && column < SpanAlignments[row].Count
      ? SpanAlignments[row][column] : null;
    internal void SetColumnSpan(int row, int column, int span, ColumnAlignment alignment, string specification = "") {
      if (span < 1) throw new System.ArgumentOutOfRangeException(nameof(span));
      while (ColumnSpans.Count <= row) ColumnSpans.Add(new List<int>());
      while (ColumnSpans[row].Count <= column) ColumnSpans[row].Add(1);
      ColumnSpans[row][column] = span;
      while (SpanAlignments.Count <= row) SpanAlignments.Add(new List<ColumnAlignment?>());
      while (SpanAlignments[row].Count <= column) SpanAlignments[row].Add(null);
      SpanAlignments[row][column] = alignment;
      while (SpanSpecifications.Count <= row) SpanSpecifications.Add(new List<string?>());
      while (SpanSpecifications[row].Count <= column) SpanSpecifications[row].Add(null);
      SpanSpecifications[row][column] = specification;
    }
  }
}
