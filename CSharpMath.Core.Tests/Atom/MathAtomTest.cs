using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using System.Linq;
using Xunit;

namespace CSharpMath.Core.Tests {
  public class MathAtomTest {
    internal static void CheckClone(MathAtom? original, MathAtom? clone) =>
      MathListTest.CheckClone(original, clone);
    internal static void CheckClone(MathList? original, MathList? clone) =>
      MathListTest.CheckClone(original, clone);
    [Fact]
    public void TestTypeName() =>
      Assert.Equal("Binary Operator", new BinaryOperator("").TypeName);
    [Fact]
    public void TestAtomInit() {
      foreach (var c in
        typeof(Accent)
        .Assembly
        .DefinedTypes
        .Where(t => t.Namespace == typeof(Accent).Namespace && t.GetCustomAttributes(
          typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false).Length == 0)
        .SelectMany(t =>
          new[] { t.GetConstructor(new System.Type[0]), t.GetConstructor(new[] { typeof(string) }) })
        .Where(c => c != null)
        .Cast<System.Reflection.ConstructorInfo>()) {
        switch (c.GetParameters().Length) {
          case 0:
            Assert.Equal("", Assert.IsAssignableFrom<MathAtom>(c.Invoke(null)).Nucleus);
            break;
          case 1 when c.DeclaringType == typeof(Table):
            var table = Assert.IsAssignableFrom<Table>(c.Invoke(new[] { "(" }));
            Assert.Equal("", table.Nucleus);
            Assert.Equal("(", table.Environment);
            break;
          case 1:
            Assert.Equal("(", Assert.IsAssignableFrom<MathAtom>(c.Invoke(new[] { "(" })).Nucleus);
            break;
          default:
            throw new Xunit.Sdk.XunitException("Unexpected parameter length");
        }
      }
    }
    [Fact]
    public void TestScripts() {
      var atom = new Open("(");
      Assert.True(atom.ScriptsAllowed);
      Assert.NotNull(atom.Subscript);
      Assert.NotNull(atom.Superscript);
    }
    [Fact]
    public void TestCopyFraction() {
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      var list = new MathList { atom, atom2, atom3 };
      var list2 = new MathList { atom3, atom2 };
      var frac = new Fraction(list, list2, false) {
        LeftDelimiter = new Boundary("a"),
        RightDelimiter = new Boundary("b")
      };

      Assert.IsType<Fraction>(frac);

      var copy = frac.Clone(false);
      CheckClone(copy, frac);
      CheckClone(copy.Numerator, frac.Numerator);
      Assert.False(copy.HasRule);
      Assert.Equal(new Boundary("a"), copy.LeftDelimiter);
      Assert.Equal(new Boundary("b"), copy.RightDelimiter);
    }
    [Fact]
    public void TestCopyRadical() {
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
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
      Assert.IsType<LargeOperator>(large);
      Assert.True(large.Limits);

      var copy = large.Clone(false);
      CheckClone(copy, large);
      Assert.Equal(copy.Limits, large.Limits);
    }
    [Fact]
    public void TestCopyInner() {
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var inner = new Inner(
        new Boundary("("),
        list,
        new Boundary(")")
      );

      Assert.IsType<Inner>(inner);

      var copy = inner.Clone(false);
      CheckClone(inner, copy);
      CheckClone(inner.InnerList, copy.InnerList);
    }
    [Fact]
    public void TestCopyOverline() {
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var over = new Overline(list);

      Assert.IsType<Overline>(over);

      var copy = over.Clone(false);
      CheckClone(copy, over);
      CheckClone(copy.InnerList, over.InnerList);
    }

    [Fact]
    public void TestCopyUnderline() {
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var under = new Underline(list);

      Assert.IsType<Underline>(under);

      var copy = under.Clone(false);
      CheckClone(copy, under);
      CheckClone(copy.InnerList, under.InnerList);
    }
    [Fact]
    public void TestCopyAccent() {
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
      var list = new MathList(atom1, atom2, atom3);
      var accent = new Accent("^", list);

      var copy = accent.Clone(false);
      CheckClone(copy, accent);
      CheckClone(copy.InnerList, accent.InnerList);
    }
    [Fact]
    public void TestCopySpace() {
      var space = new Space(3 * Length.Point);
      Assert.IsType<Space>(space);

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
    public void TestCopyOpen() {
      var open = new Open("(") {
        Subscript = { LaTeXSettings.Placeholder, LaTeXSettings.Times, LaTeXSettings.Divide },
        Superscript = { LaTeXSettings.Divide, LaTeXSettings.Times }
      };

      var clone = open.Clone(false);
      CheckClone(open, clone);
      CheckClone(open.Superscript, clone.Superscript);
      CheckClone(open.Subscript, clone.Subscript);
    }
    [Fact]
    public void TestCopyColored() {
      var colored = new Colored(System.Drawing.Color.FromArgb(255, 0, 0), new MathList(new Open("(")));
      var clone = colored.Clone(false);
      Assert.Equal(System.Drawing.Color.FromArgb(255, 0, 0), clone.Color);
      CheckClone(colored, clone);
      CheckClone(colored.InnerList, clone.InnerList);

      var colorBox = new ColorBox(System.Drawing.Color.FromArgb(128, 0, 0), new MathList(new Close(")")));
      var cloneBox = colorBox.Clone(false);
      Assert.Equal(System.Drawing.Color.FromArgb(128, 0, 0), cloneBox.Color);
      CheckClone(colorBox, cloneBox);
      CheckClone(colorBox.InnerList, cloneBox.InnerList);
    }
    [Fact]
    public void TestCreateMathTable() {
      var table = new Table();
      Assert.IsType<Table>(table);
      var atom1 = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
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
      Assert.IsType<Table>(table);
      var atom = LaTeXSettings.Placeholder;
      var atom2 = LaTeXSettings.Times;
      var atom3 = LaTeXSettings.Divide;
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
