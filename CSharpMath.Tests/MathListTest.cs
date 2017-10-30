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
  public class MathListTest {
    internal static void CheckClone(IMathAtom original, IMathAtom clone) {
      Assert.Equal(original, clone);
      Assert.False(ReferenceEquals(original, clone));
    }

    internal static void CheckClone(IMathList original, IMathList clone) {
      Assert.Equal(original, clone);
      Assert.False(ReferenceEquals(original, clone));
    }
    [Fact]
    public void TestCopy() {
      var list = new MathList();
      list.AddAtom(MathAtoms.Placeholder);
      list.AddAtom(MathAtoms.Times);
      list.AddAtom(MathAtoms.Divide);

      var list2 = new MathList();
      list2.AddAtom(MathAtoms.Divide);
      list2.AddAtom(MathAtoms.Times);

      var open = MathAtoms.Create(MathAtomType.Open, "(");
      open.Subscript = list;
      open.Superscript = list2;

      var clone = AtomCloner.Clone(open, false);
      CheckClone(open, clone);
      CheckClone(open.Superscript, clone.Superscript);
      CheckClone(open.Subscript, clone.Subscript);
    }

    [Fact]
    public void TestCopyFraction() {
      var atom = MathAtoms.Placeholder;
      var atom2 = MathAtoms.Times;
      var atom3 = MathAtoms.Divide;

      var list = new MathList();
      list.AddAtom(atom);
      list.AddAtom(atom2);
      list.AddAtom(atom3);

      var list2 = new MathList();
      list2.AddAtom(atom3);
      list2.AddAtom(atom2);

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
      var table = new MathTable();

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

  }
}
