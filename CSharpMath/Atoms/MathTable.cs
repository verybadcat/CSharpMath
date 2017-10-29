using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathTable : MathAtom, IMathTable {
    public List<ColumnAlignment> Alignments { get; } = new List<ColumnAlignment>();
    public List<List<IMathList>> Cells { get; set; } = new List<List<IMathList>>();

    public float InterColumnSpacing { get; set; }
    public float InterRowAdditionalSpacing { get; set; }

    private string _environment;

    public MathTable(string environment): base(MathAtomType.Table, "") {
      _environment = environment;
    }

    public string Environment {
      get => _environment;
      set => _environment = value;
    }


    public int NRows => Cells.Count;
    public int NColumns => (NRows == 0) ? 0 : Cells[0].Count;

    public void SetCell(IMathList list, int iRow, int iColumn) {
      while (Cells.Count < iRow) {
        Cells.Add(new List<IMathList>());
      }
      while (Cells[iRow].Count <= iColumn) {
        Cells[iRow].Add(new MathList());
      }
      Cells[iRow][iColumn] = list;
    }

    public void SetAlignment(ColumnAlignment alignment, int columnIndex) {
      while (Alignments.Count <= columnIndex) {
        Alignments.Add(ColumnAlignment.Center);
      }
      Alignments[columnIndex] = alignment;
    }

    public ColumnAlignment GetAlignment(int columnIndex) {
      if (Alignments.Count <= columnIndex) {
        return ColumnAlignment.Center;
      }
      return Alignments[columnIndex];
    }
  }
}
