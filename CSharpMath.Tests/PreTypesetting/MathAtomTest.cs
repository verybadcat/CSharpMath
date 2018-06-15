using CSharpMath.Atoms;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class MathAtomTest {
    internal static void CheckClone(IMathAtom original, IMathAtom clone)
      => MathListTest.CheckClone(original, clone);

    internal static void CheckClone(IMathList original, IMathList clone)
      => MathListTest.CheckClone(original, clone);

    [Fact]
    public void TestAtomInit() {
      var atom = MathAtoms.Create(MathAtomType.Open, "(");
      Assert.Equal(MathAtomType.Open, atom.AtomType);
      Assert.Equal("(", atom.Nucleus);

      var atom2 = MathAtoms.Create(MathAtomType.Radical, "(");
      Assert.Equal(MathAtomType.Radical, atom2.AtomType);
      Assert.Equal("(", atom2.Nucleus);
    }

    [Fact]
    public void TestScripts() {
      var atom = MathAtoms.Create(MathAtomType.Open, "(");
      Assert.True(atom.ScriptsAllowed);
      atom.Subscript = new MathList();
      Assert.NotNull(atom.Subscript);
      atom.Superscript = new MathList();
      Assert.NotNull(atom.Superscript);

      var atom2 = MathAtoms.Create(MathAtomType.Boundary, "(");
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

      var list = new MathList {
        atom,
        atom2,
        atom3
      };

      var list2 = new MathList {
        atom3,
        atom2
      };

      var frac = new Fraction(false) {
        Numerator = list,
        Denominator = list2,
        LeftDelimiter = "a",
        RightDelimiter = "b"
      };

      Assert.Equal(MathAtomType.Fraction, frac.AtomType);


      var copy = new Fraction(frac, false);
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

      var list = MathLists.WithAtoms(atom, atom2, atom3);
      var list2 = MathLists.WithAtoms(atom3, atom2);

      var radical = new Radical {
        Radicand = list,
        Degree = list2
      };

      var copy = new Radical(radical, false);

      CheckClone(copy, radical);
      CheckClone(copy.Radicand, radical.Radicand);
      CheckClone(copy.Degree, radical.Degree);
    }

    [Fact]
    public void TestCopyLargeOperator() {
      var large = new LargeOperator("lim", true);
      Assert.Equal(MathAtomType.LargeOperator, large.AtomType);
      Assert.True(large.Limits);

      var copy = new LargeOperator(large, false);
      CheckClone(copy, large);
      Assert.Equal(copy.Limits, large.Limits);
    }

    [Fact]
    public void TestCopyInner() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom1, atom2, atom3);

      var inner = new Inner {
        InnerList = list,
        LeftBoundary = MathAtoms.Create(MathAtomType.Boundary, "("),
        RightBoundary = MathAtoms.Create(MathAtomType.Boundary, ")")
      };

      Assert.Equal(MathAtomType.Inner, inner.AtomType);

      var copy = new Inner(inner, false);

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

      var list = MathLists.WithAtoms(atom1, atom2, atom3);

      var over = new Overline();
      Assert.Equal(MathAtomType.Overline, over.AtomType);
      over.InnerList = list;

      var copy = new Overline(over, false);
      CheckClone(copy, over);
      CheckClone(copy.InnerList, over.InnerList);
    }


    [Fact]
    public void TestCopyUnderline() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom1, atom2, atom3);

      var under = new Underline();
      Assert.Equal(MathAtomType.Underline, under.AtomType);
      under.InnerList = list;

      var copy = new Underline(under, false);
      CheckClone(copy, under);
      CheckClone(copy.InnerList, under.InnerList);
    }

    [Fact]
    public void TestCopyAccent() {
      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom1, atom2, atom3);

      var accent = new Accent("^");
      Assert.Equal(MathAtomType.Accent, accent.AtomType);
      accent.InnerList = list;

      var copy = new Accent(accent, false);
      CheckClone(copy, accent);
      CheckClone(copy.InnerList, accent.InnerList);
    }

    [Fact]
    public void TestCopySpace() {
      var space = new MathSpace(3);
      Assert.Equal(MathAtomType.Space, space.AtomType);

      var copy = new MathSpace(space, false);
      CheckClone(space, copy);
      Assert.Equal(3, copy.Space);
    }

    [Fact]
    public void TestCopyStyle() {
      var style = new MathStyle(LineStyle.Script);
      Assert.Equal(LineStyle.Script, style.Style);

      var clone = new MathStyle(style, false);
      CheckClone(clone, style);
      Assert.Equal(clone.Style, style.Style);
    }

    [Fact]
    public void TestCreateMathTable() {
      var table = new Table();

      Assert.Equal(MathAtomType.Table, table.AtomType);

      var atom1 = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = MathLists.WithAtoms(atom1, atom2, atom3);
      var list2 = MathLists.WithAtoms(atom3, atom2);

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
      var list = new MathList {
        atom,
        atom2,
        atom3
      };

      var list2 = new MathList {
        atom3,
        atom2
      };

      table.SetCell(list, 0, 1);
      table.SetCell(list2, 0, 2);

      table.SetAlignment(ColumnAlignment.Left, 2);
      table.SetAlignment(ColumnAlignment.Right, 1);

      var clone = AtomCloner.Clone(table, false) as Table;
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
