using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests {

  public class MathListBuilderTest {
    public static IEnumerable<(string, MathAtomType[], string)> RawTestData() {
      yield return ("x", new MathAtomType[] { MathAtomType.Variable }, "x");
      yield return ("1", new MathAtomType[] { MathAtomType.Number }, "1");
      yield return ("*", new MathAtomType[] { MathAtomType.BinaryOperator }, "*");
      yield return ("+", new MathAtomType[] { MathAtomType.BinaryOperator }, "+");
      yield return (".", new MathAtomType[] { MathAtomType.Number }, ".");
      yield return ("(", new MathAtomType[] { MathAtomType.Open }, "(");
      yield return (")", new MathAtomType[] { MathAtomType.Close }, ")");
      yield return (",", new MathAtomType[] { MathAtomType.Punctuation }, ",");
      yield return ("!", new MathAtomType[] { MathAtomType.Close }, "!");
      yield return ("=", new MathAtomType[] { MathAtomType.Relation }, "=");
      yield return ("x+2", new MathAtomType[] { MathAtomType.Variable, MathAtomType.BinaryOperator, MathAtomType.Number }, "x+2");
      yield return ("(2.3 * 8)", new MathAtomType[] { MathAtomType.Open, MathAtomType.Number, MathAtomType.Number, MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Number, MathAtomType.Close }, "(2.3*8)");
      yield return ("5{3+4}", new MathAtomType[] { MathAtomType.Number, MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Number }, "53+4"); // braces are just for grouping
      // commands
      yield return (@"\pi+\theta\geq 3", new MathAtomType[] { MathAtomType.Variable, MathAtomType.BinaryOperator, MathAtomType.Variable, MathAtomType.Relation, MathAtomType.Number }, @"\pi +\theta \geq 3");
      //aliases
      yield return (@"\pi\ne 5 \land 3", new MathAtomType[] { MathAtomType.Variable, MathAtomType.Relation, MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Number }, @"\pi \neq 5\wedge 3");
      // control space
      yield return (@"x \ y", new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary, MathAtomType.Variable }, @"x\  y");
      // spacing
      yield return (@"x \quad y \; z \! q", new MathAtomType[]
      {MathAtomType.Variable, MathAtomType.Space, MathAtomType.Variable,
        MathAtomType.Space, MathAtomType.Variable,
      MathAtomType.Space, MathAtomType.Variable}, @"x\quad y\; z\! q");
    }

    public static IEnumerable<(string, MathAtomType[][], string)> RawSuperscriptTestData() {
      yield return ("x^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable }, new MathAtomType[] { MathAtomType.Number } }, "x^2");
      yield return ("x^23", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Number }, new MathAtomType[] { MathAtomType.Number } }, "x^23");
      yield return ("x^{23}", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable }, new MathAtomType[] { MathAtomType.Number, MathAtomType.Number } }, "x^{23}");
      yield return ("x^2^3", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "x^2{}^3");
      yield return ("x^{2^3}", new MathAtomType[][] { new MathAtomType[] {MathAtomType.Variable},
        new MathAtomType[] { MathAtomType.Number },
      new MathAtomType[]{MathAtomType.Number} }, "x^{2^3}");
      yield return ("x^{^2*}", new MathAtomType[][] {
        new MathAtomType[]{MathAtomType.Variable },
        new MathAtomType[]{MathAtomType.Ordinary, MathAtomType.BinaryOperator },
        new MathAtomType[]{MathAtomType.Number } },
        "x^{{}^2*}");
      yield return ("^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^2");
      yield return ("{}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^2");
      yield return ("x^^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary }, new MathAtomType[] { } }, "x^{}{}^2");
      yield return ("5{x}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Number, MathAtomType.Variable }, new MathAtomType[] { } }, "5{x}^2");
    }

    public static IEnumerable<object[]> SuperscriptTestData() {
      foreach (var tuple in RawSuperscriptTestData()) {
        yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
      }
    }

    public static IEnumerable<object[]> SubscriptTestData() {
      foreach (var tuple in RawSuperscriptTestData()) {
        yield return new object[] {
          tuple.Item1.Replace('^', '_'),
          tuple.Item2,
          tuple.Item3.Replace('^', '_')
        };
      }
    }

    public static IEnumerable<object[]> TestData() {
      foreach (var tuple in RawTestData()) {
        yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
      }
    }

    [Theory, MemberData(nameof(TestData))]
    public void TestBuilder(string input, MathAtomType[] atomTypes, string output) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      list.ExpandGroups();
      CheckAtomTypes(list, atomTypes);


      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(output, latex);
    }

    [Theory, MemberData(nameof(SuperscriptTestData))]
    public void TestSuperscript(string input, MathAtomType[][] atomTypes, string output)
     => RunScriptTest(input, atom => atom.Superscript, atomTypes, output);

    [Theory, MemberData(nameof(SubscriptTestData))]
    public void TestSubscript(string input, MathAtomType[][] atomTypes, string output)
     => RunScriptTest(input, atom => atom.Subscript, atomTypes, output);

    private void RunScriptTest(string input, Func<IMathAtom, IMathList> scriptGetter, MathAtomType[][] atomTypes, string output) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);

      var expandedList = new MathList(list, false);
      expandedList.ExpandGroups();
      CheckAtomTypes(expandedList, atomTypes[0]);

      IMathAtom firstAtom = expandedList[0];
      var types = atomTypes[1];
      if (types.Count() > 0) {
        Assert.NotNull(scriptGetter(firstAtom));
      }
      var scriptList = scriptGetter(firstAtom);
      CheckAtomTypes(scriptList, atomTypes[1]);
      if (atomTypes.Count() == 3) {
        // one more level
        var firstScript = scriptList[0];
        var scriptScriptList = scriptGetter(firstScript);
        CheckAtomTypes(scriptScriptList, atomTypes[2]);
      }

      string latex = MathListBuilder.MathListToString(list);
      Assert.Equal(output, latex);
    }
    
    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(IMathList list, params MathAtomType[] types) {
      int atomCount = (list == null) ? 0 : list.Atoms.Count;
      Assert.Equal(types.Count(), atomCount);
      for (int i = 0; i < atomCount; i++) {
        var atom = list[i];
        Assert.NotNull(atom);
        Assert.Equal(atom.AtomType, types[i]);
      }
    }

    private void CheckAtomTypeAndNucleus(IMathAtom atom, MathAtomType type, string nucleus) {
      Assert.Equal(type, atom.AtomType);
      Assert.Equal(nucleus, atom.Nucleus);
    }

    [Fact]
    public void TestSymbols() {
      string str = @"5\times3^{2\div2}";
      var builder = new MathListBuilder(str);
      var list = builder.Build();
      Assert.NotNull(list);
      Assert.Equal(3, list.Atoms.Count());

      CheckAtomTypeAndNucleus(list[0], MathAtomType.Number, "5");
      CheckAtomTypeAndNucleus(list[1], MathAtomType.BinaryOperator, "\u00D7");
      CheckAtomTypeAndNucleus(list[2], MathAtomType.Number, "3");

      var superList = list[2].Superscript;
      Assert.NotNull(superList);
      Assert.Equal(3, superList.Atoms.Count());
      CheckAtomTypeAndNucleus(superList[0], MathAtomType.Number, "2");
      CheckAtomTypeAndNucleus(superList[1], MathAtomType.BinaryOperator, "\u00F7");
      CheckAtomTypeAndNucleus(superList[2], MathAtomType.Number, "2");
    }

    [Fact]
    void TestFraction() {
      var input = @"\frac1c";
      var list = MathLists.FromString(input);
      Assert.NotNull(list);
      Assert.Single(list);

      var fraction = list[0] as Fraction;
      Assert.NotNull(fraction);
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      var sublist = fraction.Numerator;
      Assert.Single(sublist);
      CheckAtomTypeAndNucleus(sublist[0], MathAtomType.Number, "1");

      var denominator = fraction.Denominator;
      Assert.Single(denominator);
      CheckAtomTypeAndNucleus(denominator[0], MathAtomType.Variable, "c");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\frac{1}{c}", latex);
    }

    [Fact]
    public void TestFractionInFraction() {
      var input = @"\frac1\frac23";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Fraction, "");
      var fraction = list[0] as Fraction;

      var numerator = fraction.Numerator;
      Assert.Single(numerator);
      CheckAtomTypeAndNucleus(numerator[0], MathAtomType.Number, "1");

      var denominator = fraction.Denominator;
      Assert.Single(denominator);
      CheckAtomTypeAndNucleus(denominator[0], MathAtomType.Fraction, "");
      var subFraction = denominator[0] as Fraction;

      var subNumerator = subFraction.Numerator;
      Assert.Single(subNumerator);
      CheckAtomTypeAndNucleus(subNumerator[0], MathAtomType.Number, "2");

      var subDenominator = subFraction.Denominator;
      Assert.Single(subDenominator);
      CheckAtomTypeAndNucleus(subDenominator[0], MathAtomType.Number, "3");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\frac{1}{\frac{2}{3}}", latex);

    }

    [Fact]
    public void TestSqrt() {
      var input = @"\sqrt2";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, MathAtomType.Radical, "");

      var radicand = radical.Radicand;
      Assert.Single(radicand);
      CheckAtomTypeAndNucleus(radicand[0], MathAtomType.Number, "2");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sqrt{2}", latex);
    }

    [Fact]
    public void TestSqrtInSqrt() {
      var input = @"\sqrt\sqrt2";
      var list = MathLists.FromString(input);
      Assert.Single(list);

      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, MathAtomType.Radical, "");

      var sublist = radical.Radicand;
      Assert.Single(sublist);

      CheckAtomTypeAndNucleus(sublist[0], MathAtomType.Radical, "");

      var subSubList = (sublist[0] as Radical).Radicand;
      Assert.Single(subSubList);
      CheckAtomTypeAndNucleus(subSubList[0], MathAtomType.Number, "2");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sqrt{\sqrt{2}}", latex);
    }

    [Fact]
    public void TestRadical() {
      string input = @"\sqrt[3]2";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var radical = list[0] as Radical;
      CheckAtomTypeAndNucleus(radical, MathAtomType.Radical, "");

      IMathList subList = radical.Radicand;
      Assert.Single(subList);

      var atom = subList[0];
      CheckAtomTypeAndNucleus(atom, MathAtomType.Number, "2");

      var degree = radical.Degree;
      Assert.Single(degree);
      CheckAtomTypeAndNucleus(degree[0], MathAtomType.Number, "3");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sqrt[3]{2}", latex);
    }

    public static IEnumerable<(string, MathAtomType[], int, MathAtomType[], string, string, string)> RawTestDataLeftRight() {
      var singletonList = new MathAtomType[] { MathAtomType.Inner };
      var singletonNumber = new MathAtomType[] { MathAtomType.Number };
      var singletonVariable = new MathAtomType[] { MathAtomType.Variable };
      yield return (@"\left( 2 \right)", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) ");
      // spacing
      yield return (@"\left ( 2 \right )", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) ");
      // commands
      yield return (@"\left\{ 2 \right\}", singletonList, 0, singletonNumber, @"{", @"}", @"\left\{ 2\right\} ");
      // complex commands
      yield return (@"\left\langle x \right\rangle", singletonList, 0, singletonVariable, "\u2329", "\u232A", @"\left< x\right> ");
      // bars
      yield return (@"\left| x \right\|", singletonList, 0, singletonVariable, @"|", "\u2016", @"\left| x\right\| ");
      // inner in between
      yield return (@"5 + \left( 2 \right) - 2", new MathAtomType[] { MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Inner, MathAtomType.BinaryOperator, MathAtomType.Number }, 2, singletonNumber, @"(", @")", @"5+\left( 2\right) -2");
      // long inner
      yield return (@"\left( 2 + \frac12\right)", singletonList, 0, new MathAtomType[] { MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Fraction }, @"(", @")", @"\left( 2+\frac{1}{2}\right) ");
      // nested
      yield return (@"\left[ 2 + \left|\frac{-x}{2}\right| \right]", singletonList, 0, new MathAtomType[] { MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Inner }, @"[", @"]", @"\left[ 2+\left| \frac{-x}{2}\right| \right] ");
      // With scripts
      yield return (@"\left( 2 \right)^2", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) ^2");
      // Scripts on left
      yield return (@"\left(^2 \right )", singletonList, 0, new MathAtomType[] { MathAtomType.Ordinary }, @"(", @")", @"\left( {}^2\right) ");
      // Dot
      yield return (@"\left( 2 \right.", singletonList, 0, singletonNumber, @"(", @"", @"\left( 2\right. ");

    }

    public static IEnumerable<object[]> TestDataLeftRight() {
      foreach (var tuple in RawTestDataLeftRight()) {
        yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7 };
      }
    }

    [Theory, MemberData(nameof(TestDataLeftRight))]
    public void TestLeftRight(
      string input,
      MathAtomType[] expectedOutputTypes,
      int innerIndex,
      MathAtomType[] expectedInnerTypes,
      string leftBoundary,
      string rightBoundary,
      string expectedLatex) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();

      Assert.NotNull(list);
      Assert.Null(builder.Error);

      list.ExpandGroups();
      CheckAtomTypes(list, expectedOutputTypes);
      var inner = list[innerIndex] as Inner;
      Assert.NotNull(inner);
      CheckAtomTypeAndNucleus(inner, MathAtomType.Inner, "");

      CheckAtomTypes(inner.InnerList, expectedInnerTypes);
      CheckAtomTypeAndNucleus(inner.LeftBoundary, MathAtomType.Boundary, leftBoundary);
      CheckAtomTypeAndNucleus(inner.RightBoundary, MathAtomType.Boundary, rightBoundary);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(expectedLatex, latex);
    }

    [Fact]
    public void TestOver() {
      var input = @"1 \over c";
      var list = MathLists.FromString(input);
      Assert.Single(list);

      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Number, "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "c");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\frac{1}{c}", latex);
    }

    [Fact]
    public void TestOverInParens() {
      var input = @"5 + {1 \over c} + 8";
      var list = MathLists.FromString(input);

      Assert.NotNull(list);
      Assert.Equal(5, list.Count);
      var types = new MathAtomType[] { MathAtomType.Number, MathAtomType.BinaryOperator, MathAtomType.Fraction, MathAtomType.BinaryOperator, MathAtomType.Number };
      list.ExpandGroups();
      CheckAtomTypes(list, types);

      var fraction = list[2] as Fraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");
      Assert.True(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Number, "1");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "c");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"5+\frac{1}{c}+8", latex);
    }

    [Fact]
    public void TestAtop() {
      var input = @"1 \atop c";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var fraction = list[0] as Fraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Number, "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "c");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"{1 \atop c}", latex);
    }

    [Fact]
    public void TestAtopInParens() {
      var input = @"5 + {1 \atop c} + 8";
      var list = MathLists.FromString(input);

      Assert.Equal(5, list.Count);
      var types = new MathAtomType[] {
        MathAtomType.Number,
        MathAtomType.BinaryOperator,
        MathAtomType.Fraction,
        MathAtomType.BinaryOperator,
        MathAtomType.Number
      };
      list.ExpandGroups();
      CheckAtomTypes(list, types);
      var fraction = list[2] as IFraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");
      Assert.False(fraction.HasRule);
      Assert.Null(fraction.LeftDelimiter);
      Assert.Null(fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Number, "1");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "c");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"5+{1 \atop c}+8", latex);
    }

    [Fact]
    public void TestChoose() {
      var input = @"n \choose k";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Fraction, "");

      var fraction = list[0] as IFraction;
      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Variable, "n");

      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "k");
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"{n \choose k}", latex);
    }

    [Fact]
    public void TestBrack() {
      var input = @"n \brack k";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var fraction = list[0] as IFraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");

      Assert.False(fraction.HasRule);
      Assert.Equal("[", fraction.LeftDelimiter);
      Assert.Equal("]", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Variable, "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "k");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"{n \brack k}", latex);
    }

    [Fact]
    public void TestBrace() {
      var input = @"n \brace k";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var fraction = list[0] as IFraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");

      Assert.False(fraction.HasRule);
      Assert.Equal("{", fraction.LeftDelimiter);
      Assert.Equal("}", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Variable, "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "k");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"{n \brace k}", latex);
    }

    [Fact]
    public void TestBinomial() {
      var input = @"\binom{n}{k}";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var fraction = list[0] as IFraction;
      CheckAtomTypeAndNucleus(fraction, MathAtomType.Fraction, "");

      Assert.False(fraction.HasRule);
      Assert.Equal("(", fraction.LeftDelimiter);
      Assert.Equal(")", fraction.RightDelimiter);

      Assert.Single(fraction.Numerator);
      CheckAtomTypeAndNucleus(fraction.Numerator[0], MathAtomType.Variable, "n");
      Assert.Single(fraction.Denominator);
      CheckAtomTypeAndNucleus(fraction.Denominator[0], MathAtomType.Variable, "k");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"{n \choose k}", latex);
    }

    [Fact]
    public void TestOverline() {
      var input = @"\overline 2";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var overline = list[0] as IOverline;
      CheckAtomTypeAndNucleus(overline, MathAtomType.Overline, "");

      var inner = overline.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], MathAtomType.Number, "2");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\overline{2}", latex);
    }

    [Fact]
    public void TestUnderline() {
      var input = @"\underline 2";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var underline = list[0] as IUnderline;
      CheckAtomTypeAndNucleus(underline, MathAtomType.Underline, "");

      var inner = underline.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], MathAtomType.Number, "2");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\underline{2}", latex);
    }

    [Fact]
    public void TestAccent() {
      var input = @"\bar x";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var accent = list[0] as IAccent;
      CheckAtomTypeAndNucleus(accent, MathAtomType.Accent, "\u0304");

      var inner = accent.InnerList;
      Assert.Single(inner);
      CheckAtomTypeAndNucleus(inner[0], MathAtomType.Variable, "x");

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\bar{x}", latex);
    }

    [Fact]
    public void TestMathSpace() {
      var input = @"\!";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Space, "");
      Assert.Equal(-3, (list[0] as ISpace).Length);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\! ", latex);
    }

    [Fact]
    public void TestMathStyle() {
      var input = @"\textstyle y \scriptstyle x";
      var list = MathLists.FromString(input);
      Assert.Equal(4, list.Count);

      var style = list[0] as IStyle;
      CheckAtomTypeAndNucleus(style, MathAtomType.Style, "");
      Assert.Equal(LineStyle.Text, style.LineStyle);

      var style2 = list[2] as IStyle;
      CheckAtomTypeAndNucleus(style2, MathAtomType.Style, "");
      Assert.Equal(LineStyle.Script, style2.LineStyle);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\textstyle y\scriptstyle x", latex);
    }

    [Fact]
    public void TestMatrix() {
      var input = @"\begin{matrix} x & y \\ z & w \end{matrix}";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var table = list[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
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
          var style = cell[0] as IStyle;
          Assert.Equal(MathAtomType.Style, style.AtomType);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.Equal(MathAtomType.Variable, atom.AtomType);
        }
      }
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\begin{matrix}x&y\\ z&w\end{matrix}", latex);
    }

    [Fact]
    public void TestPMatrix() {
      var input = @"\begin{pmatrix} x & y \\ z & w \end{pmatrix}";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var inner = list[0] as IMathInner;
      CheckAtomTypeAndNucleus(inner, MathAtomType.Inner, "");
      var innerList = inner.InnerList;
      CheckAtomTypeAndNucleus(inner.LeftBoundary, MathAtomType.Boundary, "(");
      CheckAtomTypeAndNucleus(inner.RightBoundary, MathAtomType.Boundary, ")");
      Assert.Single(innerList);
      var table = innerList[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
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
          var style = cell[0] as IStyle;
          Assert.Equal(MathAtomType.Style, style.AtomType);
          Assert.Equal(LineStyle.Text, style.LineStyle);

          var atom = cell[1];
          Assert.Equal(MathAtomType.Variable, atom.AtomType);
        }
      }
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\left( \begin{matrix}x&y\\ z&w\end{matrix}\right) ", latex);
    }

    [Fact]
    public void TestDefaultTable() {
      var input = @"x \\ y";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var table = list[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
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
          Assert.Equal(MathAtomType.Variable, cell[0].AtomType);
        }
      }
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"x\\ y", latex);
    }

    [Fact]
    public void TestTableWithColumns() {
      var input = @"x & y \\ z & w";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var table = list[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
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
          Assert.Equal(MathAtomType.Variable, cell[0].AtomType);
        }
      }

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"x&y\\ z&w", latex);
    }

    [Theory]
    [InlineData(@"\begin{eqalign}x&y\\ z&w\end{eqalign}")]
    [InlineData(@"\begin{split}x&y\\ z&w\end{split}")]
    [InlineData(@"\begin{aligned}x&y\\ z&w\end{aligned}")]
    public void TestEqAlign(string input) {
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var table = list[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
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
            Assert.Equal(MathAtomType.Variable, cell[0].AtomType);
          } else {
            Assert.Equal(2, cell.Count);
            Assert.Equal(MathAtomType.Ordinary, cell[0].AtomType);
            Assert.Equal(MathAtomType.Variable, cell[1].AtomType);
          }
        }
      }
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(input, latex);
    }

    [Theory]
    [InlineData(@"\begin{displaylines}x\\ y\end{displaylines}")]
    [InlineData(@"\begin{gather}x\\ y\end{gather}")]
    public void TestDisplayLines(string input) {
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var table = list[0] as IMathTable;
      CheckAtomTypeAndNucleus(table, MathAtomType.Table, "");
      Assert.Equal(1, table.InterRowAdditionalSpacing);
      Assert.Equal(0, table.InterColumnSpacing);
      Assert.Equal(2, table.NRows);
      Assert.Equal(1, table.NColumns);
      Assert.Equal(ColumnAlignment.Center, table.GetAlignment(0));
      for (int j = 0; j < 2; j++) {
        var cell = table.Cells[j][0];
        Assert.Single(cell);
        Assert.Equal(MathAtomType.Variable, cell[0].AtomType);
      }
      var latex = MathListBuilder.MathListToString(list);
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

      MathAtoms.AddLatexSymbol("lcm", MathAtoms.Operator("lcm", false));
      var builder2 = new MathListBuilder(input);
      var list2 = builder2.Build();
      list2.ExpandGroups();
      CheckAtomTypes(list2, MathAtomType.LargeOperator, MathAtomType.Open,
        MathAtomType.Variable, MathAtomType.Punctuation, MathAtomType.Variable,
        MathAtomType.Close);
      var latex = MathListBuilder.MathListToString(list2);
      Assert.Equal(@"\lcm (a,b)", latex);
    }

    [Fact]
    public void TestFontSingle() {
      var input = @"\mathbf x";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Variable, "x");
      Assert.Equal(FontStyle.Bold, list[0].FontStyle);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\mathbf{x}", latex);
    }

    [Fact]
    public void TestFontMultipleCharacters() {
      var input = @"\frak{xy}";
      var list = MathLists.FromString(input);
      Assert.Equal(2, list.Count);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Variable, "x");
      Assert.Equal(FontStyle.Fraktur, list[0].FontStyle);
      CheckAtomTypeAndNucleus(list[1], MathAtomType.Variable, "y");
      Assert.Equal(FontStyle.Fraktur, list[1].FontStyle);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\mathfrak{xy}", latex);
    }

    [Fact]
    public void TestFontOneCharacterInside() {
      var input = @"\sqrt \mathrm x y";
      var list = MathLists.FromString(input);
      Assert.Equal(2, list.Count);

      var radical = list[0] as IRadical;
      CheckAtomTypeAndNucleus(radical, MathAtomType.Radical, "");

      var sublist = radical.Radicand;
      var atom = sublist[0];
      CheckAtomTypeAndNucleus(atom, MathAtomType.Variable, "x");
      Assert.Equal(FontStyle.Roman, atom.FontStyle);

      CheckAtomTypeAndNucleus(list[1], MathAtomType.Variable, "y");
      Assert.Equal(FontStyle.Default, list[1].FontStyle);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sqrt{\mathrm{x}}y", latex);
    }

    [Fact]
    public void TestText() {
      var input = @"\text{x y}";
      var list = MathLists.FromString(input);
      Assert.Equal(3, list.Count);
      CheckAtomTypeAndNucleus(list[0], MathAtomType.Variable, @"x");
      Assert.Equal(FontStyle.Roman, list[0].FontStyle);

      CheckAtomTypeAndNucleus(list[1], MathAtomType.Ordinary, " ");
      CheckAtomTypeAndNucleus(list[2], MathAtomType.Variable, @"y");
      Assert.Equal(FontStyle.Roman, list[2].FontStyle);

      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\mathrm{x\  y}", latex);
    }

    [Fact]
    public void TestLimits() {
      var input = @"\int";
      var list = MathLists.FromString(input);

      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.Equal(MathAtomType.LargeOperator, op.AtomType);
      Assert.False(op.Limits);
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\int ", latex);

      var input2 = @"\int\limits";
      var list2 = MathLists.FromString(input2);
      Assert.Single(list2);
      var op2 = list2[0] as LargeOperator;
      Assert.Equal(MathAtomType.LargeOperator, op2.AtomType);
      Assert.True(op2.Limits);

      var latex2 = MathListBuilder.MathListToString(list2);
      Assert.Equal(@"\int \limits ", latex2);
    }

    [Fact]
    public void TestUnspecifiedLimits() {
      var input = @"\sum";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.Equal(MathAtomType.LargeOperator, op.AtomType);
      Assert.Null(op.Limits);
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sum ", latex);
    }

    [Fact]
    public void TestNoLimits() {
      var input = @"\sum\nolimits";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var op = list[0] as LargeOperator;
      Assert.Equal(MathAtomType.LargeOperator, op.AtomType);
      Assert.False(op.Limits);
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\sum \nolimits ", latex);
    }

    [Fact]
    public void TestColor() {
      var input = @"\color{#F00}a";
      var list = MathLists.FromString(input);
      Assert.Single(list);
      var op = list[0] as Color;
      Assert.Equal(MathAtomType.Color, op.AtomType);
      Assert.False(op.ScriptsAllowed);
      var latex = MathListBuilder.MathListToString(list);
      Assert.Equal(@"\color{#F00}{a}", latex);
    }
  }
}
