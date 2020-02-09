using CSharpMath.Atoms;
using CSharpMath.Atoms.Atom;
using CSharpMath.Enumerations;
using System;
using Xunit;

namespace CSharpMath.Tests.PreTypesetting {
  public class MathAtomTest {
    internal static void CheckClone(MathAtom original, MathAtom clone) =>
      MathListTest.CheckClone(original, clone);
    internal static void CheckClone(MathList original, MathList clone) =>
      MathListTest.CheckClone(original, clone);

    [Fact]
    public void TestAtomInit() {
      var atom = new Open("(");
      Assert.Equal(MathAtomType.Open, atom.AtomType);
      Assert.Equal("(", atom.Nucleus);

      var atom2 = new Boundary("(");
      Assert.Equal(MathAtomType.Boundary, atom2.AtomType);
      Assert.Equal("(", atom2.Nucleus);
    }
    [Fact]
    public void TestScripts() {
      var atom = new Open("(");
      Assert.True(atom.ScriptsAllowed);
      atom.Subscript = new MathList();
      Assert.NotNull(atom.Subscript);
      atom.Superscript = new MathList();
      Assert.NotNull(atom.Superscript);

      var atom2 = new Boundary("(");
      Assert.False(atom2.ScriptsAllowed);
      atom2.Subscript = null;
      Assert.Null(atom2.Subscript);
      atom2.Superscript = null;
      Assert.Null(atom2.Superscript);

      var list = new MathList();
      Assert.ThrowsAny<Exception>(() => atom2.Subscript = list);
      Assert.ThrowsAny<Exception>(() => atom2.Superscript = list);
    }
    [Fact]
    public void TestCopyFraction() {
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList { atom, atom2, atom3 };
      var list2 = new MathList { atom3, atom2 };
      var frac = new Fraction(false) {
        Numerator = list,
        Denominator = list2,
        LeftDelimiter = "a",
        RightDelimiter = "b"
      };

      Assert.Equal(MathAtomType.Fraction, frac.AtomType);

      var copy = frac.Clone(false);
      CheckClone(copy, frac);
      CheckClone(copy.Numerator, frac.Numerator);
      Assert.False(copy.HasRule);
      Assert.Equal("a", copy.LeftDelimiter);
      Assert.Equal("b", copy.RightDelimiter);
    }
    [Fact]
    public void TestCopyRadical() {
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom, atom2, atom3);
      var list2 = new MathList(atom3, atom2);
      var radical = new Radical(list2, list);

      var copy = radical.Clone(false);
      CheckClone(copy, radical);
      CheckClone(copy.Radicand, radical.Radicand);
      CheckClone(copy.Degree, radical.Degree);
    }
    [Fact]
    public void TestCopyLargeOperator() {
      var large = new LargeOperator("lim", true);
      Assert.Equal(MathAtomType.LargeOperator, large.AtomType);
      Assert.True(large.Limits);

      var copy = large.Clone(false);
      CheckClone(copy, large);
      Assert.Equal(copy.Limits, large.Limits);
    }
    [Fact]
    public void TestCopyInner() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var inner = new Inner(list) {
        LeftBoundary = new Boundary("("),
        RightBoundary = new Boundary(")")
      };

      Assert.Equal(MathAtomType.Inner, inner.AtomType);

      var copy = inner.Clone(false);
      CheckClone(inner, copy);
      CheckClone(inner.InnerList, copy.InnerList);
      CheckClone(inner.LeftBoundary, copy.LeftBoundary);
      CheckClone(inner.RightBoundary, copy.RightBoundary);
    }
    [Fact]
    public void TestCopyOverline() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var over = new Overline(list);

      Assert.Equal(MathAtomType.Overline, over.AtomType);

      var copy = over.Clone(false);
      CheckClone(copy, over);
      CheckClone(copy.InnerList, over.InnerList);
    }

    [Fact]
    public void TestCopyUnderline() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var under = new Underline(list);

      Assert.Equal(MathAtomType.Underline, under.AtomType);

      var copy = under.Clone(false);
      CheckClone(copy, under);
      CheckClone(copy.InnerList, under.InnerList);
    }
    [Fact]
    public void TestCopyAccent() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var accent = new Accent("^");

      Assert.Equal(MathAtomType.Accent, accent.AtomType);
      accent.InnerList = list;

      var copy = accent.Clone(false);
      CheckClone(copy, accent);
      CheckClone(copy.InnerList, accent.InnerList);
    }
    [Fact]
    public void TestCopySpace() {
      var space = new Space(3 * CSharpMath.Structures.Space.Point);
      Assert.Equal(MathAtomType.Space, space.AtomType);

      var copy = space.Clone(false);
      CheckClone(space, copy);
      Assert.Equal(3, copy.Length);
      Assert.False(copy.IsMu);
    }
    [Fact]
    public void TestCopyStyle() {
      var style = new Style(LineStyle.Script);
      Assert.Equal(LineStyle.Script, style.LineStyle);

      var clone = style.Clone(false);
      CheckClone(clone, style);
      Assert.Equal(clone.LineStyle, style.LineStyle);
    }
    [Fact]
    public void TestCreateMathTable() {
      var table = new Table();
      Assert.Equal(MathAtomType.Table, table.AtomType);
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var list2 = new MathList(atom3, atom2);

      table.SetCell(list, 3, 2);
      table.SetCell(list2, 1, 0);
      table.SetAlignment(ColumnAlignment.Left, 2);
      table.SetAlignment(ColumnAlignment.Right, 1);

      Assert.Equal(4, table.Cells.Count);
      Assert.Empty(table.Cells[0]);
      Assert.Single(table.Cells[1]);
      Assert.Empty(table.Cells[2]);
      Assert.Equal(3, table.Cells[3].Count);

      Assert.Equal(2, table.Cells[1][0].Atoms.Count);
      Assert.Equal(list2, table.Cells[1][0]);
      Assert.Empty(table.Cells[3][0].Atoms);
      Assert.Empty(table.Cells[3][1].Atoms);
      Assert.Equal(list, table.Cells[3][2]);

      Assert.Equal(4, table.NRows);
      Assert.Equal(3, table.NColumns);
      Assert.Equal(3, table.Alignments.Count);
      Assert.Equal(ColumnAlignment.Center, table.Alignments[0]);
      Assert.Equal(ColumnAlignment.Right, table.Alignments[1]);
      Assert.Equal(ColumnAlignment.Left, table.Alignments[2]);
    }
    [Fact]
    public void TestCopyMathTable() {
      var table = new Table();
      Assert.Equal(MathAtomType.Table, table.AtomType);
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;
      var list = new MathList { atom, atom2, atom3 };
      var list2 = new MathList { atom3, atom2 };

      table.SetCell(list, 0, 1);
      table.SetCell(list2, 0, 2);
      table.SetAlignment(ColumnAlignment.Left, 2);
      table.SetAlignment(ColumnAlignment.Right, 1);

      var clone = table.Clone(false);
      CheckClone(table, clone);
      Assert.Equal(clone.InterColumnSpacing, table.InterColumnSpacing);
      Assert.Equal(clone.Alignments, table.Alignments);
      Assert.False(ReferenceEquals(clone.Alignments, table.Alignments));
      Assert.False(ReferenceEquals(clone.Cells, table.Cells));
      Assert.False(ReferenceEquals(clone.Cells[0], table.Cells[0]));
      Assert.Equal(clone.Cells[0].Count, table.Cells[0].Count);
      Assert.Empty(clone.Cells[0][0]);
      CheckClone(table.Cells[0][1], clone.Cells[0][1]);
      CheckClone(table.Cells[0][2], clone.Cells[0][2]);
      Assert.False(ReferenceEquals(clone.Cells[0][0], table.Cells[0][0]));
    }
  }
}
