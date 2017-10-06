using CSharpMath.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Interfaces {
  public interface IMathTable: IMathAtom {
    List<ColumnAlignment> Alignments { get; }
    List<List<IMathList>> Cells { get; }
    /// <summary>
    /// The name of the environment that this table denotes
    /// </summary>
    string Environment { get; set; }
    /// <summary>
    /// Space between columns in mu units.
    /// </summary>
    float InterColumnSpacing { get; set; }
    /// <summary>
    /// Additional spacing between rows in jots (one jot is 0.3 times font size).
    /// </summary>
    float InterRowSpacing { get; set; }

    void SetCell(IMathList cellContent, int row, int column);
    void SetAlignment(ColumnAlignment alignment, int column);
    /// <summary>
    /// Number of columns
    /// </summary>
    int NColumns { get; set; }
    /// <summary>
    /// Number of rows
    /// </summary>
    int NRows { get; set; }
  }
}
