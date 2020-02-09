using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CSharpMath.Atoms;
using CSharpMath.Atoms.Atom;

namespace CSharpMath.Tests.PreTypesetting {

  public class MathListBuilderTest {
    public static IEnumerable<(string, Type[], string)> RawTestData() => new[] {
      ("x", new[] { typeof(Variable) }, "x"),
      ("1", new[] { typeof(Number) }, "1"),
      ("*", new[] { typeof(BinaryOperator) }, "*"),
      ("+", new[] { typeof(BinaryOperator) }, "+"),
      (".", new[] { typeof(Number) }, "."),
      ("(", new[] { typeof(Open) }, "("),
      (")", new[] { typeof(Close) }, ")"),
      (",", new[] { typeof(Punctuation) }, ","),
      ("!", new[] { typeof(Close) }, "!"),
      ("=", new[] { typeof(Relation) }, "="),
      ("x+2", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Number) }, "x+2"),
      ("(2.3 * 8)", new[] { typeof(Open), typeof(Number), typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number), typeof(Close) }, "(2.3*8)"),
      ("5{3+4}", new[] { typeof(Number), typeof(Number), typeof(BinaryOperator), typeof(Number) }, "53+4"), // braces are just for grouping
      // commands
      (@"\pi+\theta\geq 3", new[] { typeof(Variable), typeof(BinaryOperator), typeof(Variable), typeof(Relation), typeof(Number) }, @"\pi +\theta \geq 3"),
      // aliases
      (@"\pi\ne 5 \land 3", new[] { typeof(Variable), typeof(Relation), typeof(Number), typeof(BinaryOperator), typeof(Number) }, @"\pi \neq 5\wedge 3"),
      // control space
      (@"x \ y", new[] { typeof(Variable), typeof(Ordinary), typeof(Variable) }, @"x\  y"),
      // spacing
      (@"x \quad y \; z \! q", new[] { typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable), typeof(Space), typeof(Variable)}, @"x\quad y\; z\! q")
    };

    public static IEnumerable<(string, Type[][], string)> RawSuperscriptTestData() => new[] {
      ("x^2", new[] { new[] { typeof(Variable) }, new[] { typeof(Number) } }, "x^2"),
      ("x^23", new[] { new[] { typeof(Variable), typeof(Number) }, new[] { typeof(Number) } }, "x^23"),
      ("x^{23}", new[] { new[] { typeof(Variable) }, new[] { typeof(Number), typeof(Number) } }, "x^{23}"),
      ("x^2^3", new[] { new[] { typeof(Variable), typeof(Ordinary) }, new[] { typeof(Number) } }, "x^2{}^3"),
      ("x^{2^3}", new[] { new[] { typeof(Variable)}, new[] { typeof(Number) }, new[] {typeof(Number)} }, "x^{2^3}"),
      ("x^{^2*}", new[] { new[] { typeof(Variable) }, new[] { typeof(Ordinary), typeof(BinaryOperator) }, new[] { typeof(Number) } }, "x^{{}^2*}"),
      ("^2", new[] { new[] { typeof(Ordinary) }, new[] { typeof(Number) } }, "{}^2"),
      ("{}^2", new[] { new[] { typeof(Ordinary) }, new[] { typeof(Number) } }, "{}^2"),
      ("x^^2", new[] { new[] { typeof(Variable), typeof(Ordinary) }, new Type[] { } }, "x^{}{}^2"),
      ("5{x}^2", new[] { new[] { typeof(Number), typeof(Variable) }, new Type[] { } }, "5x^2")
    };

    public static IEnumerable<object[]> SuperscriptTestData() {
      foreach (var (i1, i2, i3) in RawSuperscriptTestData())
        yield return new object[] { i1, i2, i3 };
    }

    public static IEnumerable<object[]> SubscriptTestData() {
      foreach (var (i1, i2, i3) in RawSuperscriptTestData())
        yield return new object[] { i1.Replace('^', '_'), i2, i3.Replace('^', '_') };
    }

    public static IEnumerable<object[]> TestData() {
      foreach (var (i1, i2, i3) in RawTestData())
        yield return new object[] { i1, i2, i3 };
    }

    [Theory, MemberData(nameof(TestData))]
    public void TestBuilder(string input, Type[] atomTypes, string output) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      CheckAtomTypes(list, atomTypes);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(output, latex);
    }

    [Theory, MemberData(nameof(SuperscriptTestData))]
    public void TestSuperscript(string input, Type[][] atomTypes, string output) =>
      RunScriptTest(input, atom => atom.Superscript, atomTypes, output);

    [Theory, MemberData(nameof(SubscriptTestData))]
    public void TestSubscript(string input, Type[][] atomTypes, string output) =>
      RunScriptTest(input, atom => atom.Subscript, atomTypes, output);

    private void RunScriptTest(string input, Func<MathAtom, MathList> scriptGetter, Type[][] atomTypes, string output) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      var expandedList = list.Clone(false);
      CheckAtomTypes(expandedList, atomTypes[0]);

      var firstAtom = expandedList[0];
      var types = atomTypes[1];
      if (types.Count() > 0)
        Assert.NotNull(scriptGetter(firstAtom));

      var scriptList = scriptGetter(firstAtom);
      CheckAtomTypes(scriptList, atomTypes[1]);
      if (atomTypes.Count() == 3) {
        // one more level
        var firstScript = scriptList[0];
        var scriptScriptList = scriptGetter(firstScript);
        CheckAtomTypes(scriptScriptList, atomTypes[2]);
      }

      string latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(output, latex);
    }
    
    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(MathList list, params Type[] types) {
      int atomCount = (list == null) ? 0 : list.Atoms.Count;
      Assert.Equal(types.Count(), atomCount);
      for (int i = 0; i < atomCount; i++) {
        var atom = list[i];
        Assert.NotNull(atom);
        Assert.IsType(types[i], atom);
      }
    }

    private void CheckAtomTypeAndNucleus(MathAtom atom, Type type, string nucleus) {
      Assert.IsType(type, atom);
      Assert.Equal(nucleus, atom.Nucleus);
    }

    [Fact]
    public void TestSymbols() {
      string str = @"5\times3^{2\div2}";
      var builder = new MathListBuilder(str);
      var list = builder.Build();
      Assert.NotNull(list);
      Assert.Equal(3, list.Atoms.Count());

      CheckAtomTypeAndNucleus(list[0], typeof(Number), "5");
      CheckAtomTypeAndNucleus(list[1], typeof(BinaryOperator), "\u00D7");
      CheckAtomTypeAndNucleus(list[2], typeof(Number), "3");

      var superList = list[2].Superscript;
      Assert.NotNull(superList);
      Assert.Equal(3, superList.Atoms.Count());
      CheckAtomTypeAndNucleus(superList[0], typeof(Number), "2");
      CheckAtomTypeAndNucleus(superList[1], typeof(BinaryOperator), "\u00F7");
      CheckAtomTypeAndNucleus(superList[2], typeof(Number), "2");
    }

    [Fact]
    public void TestFraction() {
      var input = @"\frac1c";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.NotNull(list);
      Assert.Single(list);

      var fraction = list[0] as Fraction;
      Assert.NotNull(fraction);
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      var sublist = fraction.Numerator;
      Assert.Single(sublist);
      CheckAtomTypeAndNucleus(sublist[0], typeof(Number), "1");

      var denominator = fraction.Denominator;
      Assert.Single(denominator);
      CheckAtomTypeAndNucleus(denominator[0], typeof(Variable), "c");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\frac{1}{c}", latex);
    }

    [Fact]
    public void TestFractionInFraction() {
      var input = @"\frac1\frac23";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], typeof(Fraction), "");
      var fraction = list[0] as Fraction;

      var numerator = fraction.Numerator;
      Assert.Single(numerator);
      CheckAtomTypeAndNucleus(numerator[0], typeof(Number), "1");

      var denominator = fraction.Denominator;
      Assert.Single(denominator);
      CheckAtomTypeAndNucleus(denominator[0], typeof(Fraction), "");
      var subFraction = denominator[0] as Fraction;

      var subNumerator = subFraction.Numerator;
      Assert.Single(subNumerator);
      CheckAtomTypeAndNucleus(subNumerator[0], typeof(Number), "2");

      var subDenominator = subFraction.Denominator;
      Assert.Single(subDenominator);
      CheckAtomTypeAndNucleus(subDenominator[0], typeof(Number), "3");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\frac{1}{\frac{2}{3}}", latex);

    }

    [Fact]
    public void TestSqrt() {
      var input = @"\sqrt2";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, typeof(Radical), "");

      var radicand = radical.Radicand;
      Assert.Single(radicand);
      CheckAtomTypeAndNucleus(radicand[0], typeof(Number), "2");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sqrt{2}", latex);
    }

    [Fact]
    public void TestSqrtInSqrt() {
      var input = @"\sqrt\sqrt2";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);

      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, typeof(Radical), "");

      var sublist = radical.Radicand;
      Assert.Single(sublist);

      CheckAtomTypeAndNucleus(sublist[0], typeof(Radical), "");

      var subSubList = (sublist[0] as Radical).Radicand;
      Assert.Single(subSubList);
      CheckAtomTypeAndNucleus(subSubList[0], typeof(Number), "2");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sqrt{\sqrt{2}}", latex);
    }

    [Fact]
    public void TestRadical() {
      string input = @"\sqrt[3]2";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, typeof(Radical), "");

      MathList subList = radical.Radicand;
      Assert.Single(subList);

      var atom = subList[0];
      CheckAtomTypeAndNucleus(atom, typeof(Number), "2");

      var degree = radical.Degree;
      Assert.Single(degree);
      CheckAtomTypeAndNucleus(degree[0], typeof(Number), "3");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sqrt[3]{2}", latex);
    }

    public static IEnumerable<(string, Type[], int, Type[], string, string, string)> RawTestDataLeftRight() {
      var singletonList = new[] { typeof(Inner) };
      var singletonNumber = new[] { typeof(Number) };
      var singletonVariable = new[] { typeof(Variable) };
      return new[] {
        (@"\left( 2 \right)", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) "),
        // spacing
        (@"\left ( 2 \right )", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) "),
        // commands
        (@"\left\{ 2 \right\}", singletonList, 0, singletonNumber, @"{", @"}", @"\left\{ 2\right\} "),
        // complex commands
        (@"\left\langle x \right\rangle", singletonList, 0, singletonVariable, "\u2329", "\u232A", @"\left< x\right> "),
        // bars
        (@"\left| x \right\|", singletonList, 0, singletonVariable, @"|", "\u2016", @"\left| x\right\| "),
        // inner in between
        (@"5 + \left( 2 \right) - 2", new[] { typeof(Number), typeof(BinaryOperator), typeof(Inner), typeof(BinaryOperator), typeof(Number) }, 2, singletonNumber, @"(", @")", @"5+\left( 2\right) -2"),
        // long inner
        (@"\left( 2 + \frac12\right)", singletonList, 0, new[] { typeof(Number), typeof(BinaryOperator), typeof(Fraction) }, @"(", @")", @"\left( 2+\frac{1}{2}\right) "),
        // nested
        (@"\left[ 2 + \left|\frac{-x}{2}\right| \right]", singletonList, 0, new[] { typeof(Number), typeof(BinaryOperator), typeof(Inner) }, @"[", @"]", @"\left[ 2+\left| \frac{-x}{2}\right| \right] "),
        // With scripts
        (@"\left( 2 \right)^2", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) ^2"),
        // Scripts on left
        (@"\left(^2 \right )", singletonList, 0, new[] { typeof(Ordinary) }, @"(", @")", @"\left( {}^2\right) "),
        // Dot
        (@"\left( 2 \right.", singletonList, 0, singletonNumber, @"(", @"", @"\left( 2\right. ")
      };
    }

    public static IEnumerable<object[]> TestDataLeftRight() {
      foreach (var (i1, i2, i3, i4, i5, i6, i7) in RawTestDataLeftRight())
        yield return new object[] { i1, i2, i3, i4, i5, i6, i7 };
    }

    [Theory, MemberData(nameof(TestDataLeftRight))]
    public void TestLeftRight(
      string input,
      Type[] expectedOutputTypes,
      int innerIndex,
      Type[] expectedInnerTypes,
      string leftBoundary,
      string rightBoundary,
      string expectedLatex) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();

      Assert.NotNull(list);
      Assert.Null(builder.Error);

      CheckAtomTypes(list, expectedOutputTypes);
      var inner = list[innerIndex] as Inner;
      Assert.NotNull(inner);
      CheckAtomTypeAndNucleus(inner, typeof(Inner), "");

      CheckAtomTypes(inner.InnerList, expectedInnerTypes);
      CheckAtomTypeAndNucleus(inner.LeftBoundary, typeof(Boundary), leftBoundary);
      CheckAtomTypeAndNucleus(inner.RightBoundary, typeof(Boundary), rightBoundary);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(expectedLatex, latex);
    }

    [Fact]
    public void TestOver() {
      var input = @"1 \over c";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);

      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Number), "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "c");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\frac{1}{c}", latex);
    }

    [Fact]
    public void TestOverInParens() {
      var input = @"5 + {1 \over c} + 8";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.NotNull(list);
      Assert.Equal(5, list.Count);
      var types = new Type[] { typeof(Number), typeof(BinaryOperator), typeof(Fraction), typeof(BinaryOperator), typeof(Number) };
      CheckAtomTypes(list, types);

      var fraction = list[2] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Number), "1");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "c");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"5+\frac{1}{c}+8", latex);
    }

    [Fact]
    public void TestAtop() {
      var input = @"1 \atop c";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Number), "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "c");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"{1 \atop c}", latex);
    }

    [Fact]
    public void TestAtopInParens() {
      var input = @"5 + {1 \atop c} + 8";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Equal(5, list.Count);
      var types = new Type[] {
        typeof(Number),
        typeof(BinaryOperator),
        typeof(Fraction),
        typeof(BinaryOperator),
        typeof(Number)
      };
      CheckAtomTypes(list, types);
      var fraction = list[2] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Number), "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "c");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"5+{1 \atop c}+8", latex);
    }

    [Fact]
    public void TestChoose() {
      var input = @"n \choose k";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], typeof(Fraction), "");

      var fraction = list[0] as Fraction;
      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Variable), "n");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "k");
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"{n \choose k}", latex);
    }

    [Fact]
    public void TestBrack() {
      var input = @"n \brack k";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");

      Assert.False(fraction.HasRule);
      Assert.Equal("[", fraction.LeftDelimiter);
      Assert.Equal("]", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Variable), "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "k");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"{n \brack k}", latex);
    }

    [Fact]
    public void TestBrace() {
      var input = @"n \brace k";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");

      Assert.False(fraction.HasRule);
      Assert.Equal("{", fraction.LeftDelimiter);
      Assert.Equal("}", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Variable), "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "k");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"{n \brace k}", latex);
    }

    [Fact]
    public void TestBinomial() {
      var input = @"\binom{n}{k}";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, typeof(Fraction), "");

      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], typeof(Variable), "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], typeof(Variable), "k");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"{n \choose k}", latex);
    }

    [Fact]
    public void TestOverline() {
      var input = @"\overline 2";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var overline = list[0] as Overline;
      CheckAtomTypeAndNucleus(overline, typeof(Overline), "");

      var inner = overline.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], typeof(Number), "2");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\overline{2}", latex);
    }

    [Fact]
    public void TestUnderline() {
      var input = @"\underline 2";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var underline = list[0] as Underline;
      CheckAtomTypeAndNucleus(underline, typeof(Underline), "");

      var inner = underline.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], typeof(Number), "2");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\underline{2}", latex);
    }

    [Fact]
    public void TestAccent() {
      var input = @"\bar x";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var accent = list[0] as Accent;
      CheckAtomTypeAndNucleus(accent, typeof(Accent), "\u0304");

      var inner = accent.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], typeof(Variable), "x");

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\bar{x}", latex);
    }

    [Fact]
    public void TestMathSpace() {
      var input = @"\!";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], typeof(Space), "");
      Assert.Equal(-3, (list[0] as Space).Length);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\! ", latex);
    }

    [Fact]
    public void TestMathStyle() {
      var input = @"\textstyle y \scriptstyle x";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Equal(4, list.Count);

      var style = list[0] as Style;
      CheckAtomTypeAndNucleus(style, typeof(Style), "");
      Assert.Equal(LineStyle.Text, style.LineStyle);

      var style2 = list[2] as Style;
      CheckAtomTypeAndNucleus(style2, typeof(Style), "");
      Assert.Equal(LineStyle.Script, style2.LineStyle);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\textstyle y\scriptstyle x", latex);
    }

    [Fact]
    public void TestMatrix() {
      var input = @"\begin{matrix} x & y \\ z & w \end{matrix}";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Equal("matrix", table.Environment);
      Assert.Equal(0, table.InterRowAdditionalSpacing);
      Assert.Equal(18, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(ColumnAlignment.Center, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Equal(2, cell.Count);
          var style = cell[0] as Style;
          Assert.IsType<Style>(style);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.IsType<Variable>(atom);
        }
      }
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\begin{matrix}x&y\\ z&w\end{matrix}", latex);
    }

    [Fact]
    public void TestPMatrix() {
      var input = @"\begin{pmatrix} x & y \\ z & w \end{pmatrix}";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var inner = list[0] as Inner;
      CheckAtomTypeAndNucleus(inner, typeof(Inner), "");
      var innerList = inner.InnerList;
      CheckAtomTypeAndNucleus(inner.LeftBoundary, typeof(Boundary), "(");
      CheckAtomTypeAndNucleus(inner.RightBoundary, typeof(Boundary), ")");
      Assert.Single(innerList);
      var table = innerList[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Equal("matrix", table.Environment);
      Assert.Equal(0, table.InterRowAdditionalSpacing);
      Assert.Equal(18, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(ColumnAlignment.Center, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Equal(2, cell.Count);
          var style = cell[0] as Style;
          Assert.IsType<Style>(style);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.IsType<Variable>(atom);
        }
      }
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\left( \begin{matrix}x&y\\ z&w\end{matrix}\right) ", latex);
    }

    [Fact]
    public void TestDefaultTable() {
      var input = @"x \\ y";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      for (int i = 0; i < 1; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Single(cell);
          Assert.IsType<Variable>(cell[0]);
        }
      }
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"x\\ y", latex);
    }

    [Fact]
    public void TestTableWithColumns() {
      var input = @"x & y \\ z & w";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Null(table.Environment);
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);

      for (int i = 0; i < 2; i++) {
        Assert.Equal(ColumnAlignment.Left, table.GetAlignment(i));
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          Assert.Single(cell);
          Assert.IsType<Variable>(cell[0]);
        }
      }

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"x&y\\ z&w", latex);
    }

    [Theory]
    [InlineData(@"\begin{eqalign}x&y\\ z&w\end{eqalign}")]
    [InlineData(@"\begin{split}x&y\\ z&w\end{split}")]
    [InlineData(@"\begin{aligned}x&y\\ z&w\end{aligned}")]
    public void TestEqAlign(string input) {
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(2, table.NColumns);
      for (int i = 0; i < 2; i++) {
        var alignment = table.GetAlignment(i);
        Assert.Equal(i == 0 ? ColumnAlignment.Right : ColumnAlignment.Left, alignment);
        for (int j = 0; j < 2; j++) {
          var cell = table.Cells[j][i];
          if (i == 0) {
            Assert.Single(cell);
            Assert.IsType<Variable>(cell[0]);
          } else {
            Assert.Equal(2, cell.Count);
            Assert.IsType<Ordinary>(cell[0]);
            Assert.IsType<Variable>(cell[1]);
          }
        }
      }
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(input, latex);
    }

    [Theory]
    [InlineData(@"\begin{displaylines}x\\ y\end{displaylines}")]
    [InlineData(@"\begin{gather}x\\ y\end{gather}")]
    public void TestDisplayLines(string input) {
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var table = list[0] as Table;
      CheckAtomTypeAndNucleus(table, typeof(Table), "");
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, table.GetAlignment(0));
      for (int j = 0; j < 2; j++) {
        var cell = table.Cells[j][0];
        Assert.Single(cell);
        Assert.IsType<Variable>(cell[0]);
      }
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(input, latex);
    }

    [Theory]
    [InlineData(@"}a")]
    [InlineData(@"\notacommand")]
    [InlineData(@"\sqrt[5+3")]
    [InlineData(@"{5+3")]
    [InlineData(@"5+3}")]
    [InlineData(@"{1+\frac{3+2")]
    [InlineData(@"1+\left")]
    [InlineData(@"\left(\frac12\right")]
    [InlineData(@"\left 5 + 3 \right)")]
    [InlineData(@"\left(\frac12\right + 3")]
    [InlineData(@"\left\lmoustache 5 + 3 \right)")]
    [InlineData(@"\left(\frac12\right\rmoustache + 3")]
    [InlineData(@"5 + 3 \right)")]
    [InlineData(@"\left(\frac12")]
    [InlineData(@"\left(5 + \left| \frac12 \right)")]
    [InlineData(@"5+ \left|\frac12\right| \right)")]
    [InlineData(@"\begin matrix \end matrix")] // missing {
    [InlineData(@"\begin")] // missing {
    [InlineData(@"\begin{")] // missing }
    [InlineData(@"\begin{matrix parens}")] // missing } (no spaces in env)
    [InlineData(@"\begin{matrix} x")]
    [InlineData(@"\begin{matrix} x \end")] // missing {
    [InlineData(@"\begin{matrix} x \end + 3")] // missing {
    [InlineData(@"\begin{matrix} x \end{")] // missing }
    [InlineData(@"\begin{matrix} x \end{matrix + 3")]// missing }
    [InlineData(@"\begin{matrix} x \end{pmatrix}")]
    [InlineData(@"x \end{matrix}")]
    [InlineData(@"\begin{notanenv} x \end{notanenv}")]
    [InlineData(@"\begin{matrix} \notacommand \end{matrix}")]
    [InlineData(@"\begin{displaylines} x & y \end{displaylines}")]
    [InlineData(@"\begin{eqalign} x \end{eqalign}")]
    [InlineData(@"\nolimits")]
    [InlineData(@"\frac\limits{1}{2}")]
    public void TestErrors(string badInput) {
      var builder = new MathListBuilder(badInput);
      var list = builder.Build();
      Assert.Null(list);
      Assert.NotNull(builder.Error);
    }

    [Fact]
    public void TestCustom() {
      var input = @"\lcm(a,b)";
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(list);
      Assert.NotNull(builder.Error);

      MathAtoms.AddLatexSymbol("lcm", new LargeOperator("lcm", false));
      var builder2 = new MathListBuilder(input);
      var list2 = builder2.Build();
      CheckAtomTypes(list2, typeof(LargeOperator), typeof(Open),
        typeof(Variable), typeof(Punctuation), typeof(Variable),
        typeof(Close));
      var latex = MathListBuilder.MathListToLaTeX(list2);
      Assert.Equal(@"\lcm (a,b)", latex);
    }

    [Fact]
    public void TestFontSingle() {
      var input = @"\mathbf x";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], typeof(Variable), "x");
      Assert.Equal(FontStyle.Bold, list[0].FontStyle);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\mathbf{x}", latex);
    }

    [Fact]
    public void TestFontMultipleCharacters() {
      var input = @"\frak{xy}";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Equal(2, list.Count);
      CheckAtomTypeAndNucleus(list[0], typeof(Variable), "x");
      Assert.Equal(FontStyle.Fraktur, list[0].FontStyle);
      CheckAtomTypeAndNucleus(list[1], typeof(Variable), "y");
      Assert.Equal(FontStyle.Fraktur, list[1].FontStyle);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\mathfrak{xy}", latex);
    }

    [Fact]
    public void TestFontOneCharacterInside() {
      var input = @"\sqrt \mathrm x y";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Equal(2, list.Count);

      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, typeof(Radical), "");

      var sublist = radical.Radicand;
      var atom = sublist[0];
      CheckAtomTypeAndNucleus(atom, typeof(Variable), "x");
      Assert.Equal(FontStyle.Roman, atom.FontStyle);

      CheckAtomTypeAndNucleus(list[1], typeof(Variable), "y");
      Assert.Equal(FontStyle.Default, list[1].FontStyle);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sqrt{\mathrm{x}}y", latex);
    }

    [Fact]
    public void TestText() {
      var input = @"\text{x y}";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Equal(3, list.Count);
      CheckAtomTypeAndNucleus(list[0], typeof(Variable), @"x");
      Assert.Equal(FontStyle.Roman, list[0].FontStyle);

      CheckAtomTypeAndNucleus(list[1], typeof(Ordinary), " ");
      CheckAtomTypeAndNucleus(list[2], typeof(Variable), @"y");
      Assert.Equal(FontStyle.Roman, list[2].FontStyle);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\mathrm{x\  y}", latex);
    }

    [Fact]
    public void TestLimits() {
      var input = @"\int";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.False(op.Limits);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\int ", latex);

      var input2 = @"\int\limits";
      var list2 = MathListBuilder.MathListFromLaTeX(input2);
      Assert.Single(list2);
      var op2 = list2[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op2);
      Assert.True(op2.Limits);

      var latex2 = MathListBuilder.MathListToLaTeX(list2);
      Assert.Equal(@"\int \limits ", latex2);
    }

    [Fact]
    public void TestUnspecifiedLimits() {
      var input = @"\sum";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.Null(op.Limits);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sum ", latex);
    }

    [Fact]
    public void TestNoLimits() {
      var input = @"\sum\nolimits";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.IsType<LargeOperator>(op);
      Assert.False(op.Limits);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\sum \nolimits ", latex);
    }

    [Fact]
    public void TestColor() {
      var input = @"\color{#F00}a";
      var list = MathListBuilder.MathListFromLaTeX(input);
      Assert.Single(list);
      var op = list[0] as Color;
      Assert.IsType<Color>(op);
      Assert.False(op.ScriptsAllowed);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\color{#F00}{a}", latex);
    }

    [Fact]
    public void TestSingleColumnArray() {
      var input = @"\begin{array}{l}a=14\\b=15\end{array}";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\begin{array}{l}a=14\\ b=15\end{array}", latex);
    }

    [Fact]
    public void TestDoubleColumnArray() {
      var input = @"\begin{array}{lr}x^2&\:x<0\\x^3&\:x\geq0\end{array}";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\begin{array}{lr}x^2&\: x<0\\ x^3&\: x\geq 0\end{array}", latex);
    }

    [Fact]
    public void TestListToString_integral_a_to_b() {
      var input = @"\int_a^b";
      var list = MathListBuilder.MathListFromLaTeX(input);

      Assert.Single(list);
      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\int _a^b", latex);
    }

    [Fact]
    public void TestListToString_integral() {
      var input = @"\int_wdf=\int_{\partial w}f";
      var list = MathListBuilder.MathListFromLaTeX(input);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\int _wdf=\int _{\partial w}f", latex);
    }

    [Fact]
    public void TestMatrixListToString() {
      var input = @"\begin{ vmatrix}\sin(x) &\cos(x)\\-\cos(x) &\sin(x)\end{ vmatrix}= 1";
      var list = MathListBuilder.MathListFromLaTeX(input);

      var latex = MathListBuilder.MathListToLaTeX(list);
      Assert.Equal(@"\left| \begin{matrix}\sin (x)&\cos (x)\\ -\cos (x)&\sin (x)\end{matrix}\right| =1", latex);
    }

  }
}
