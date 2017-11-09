using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CSharpMath.Atoms;
using CSharpMath.Interfaces;

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
      yield return ("x^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable }, new MathAtomType[] { MathAtomType.Number } }, "x^{2}");
      yield return ("x^23", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Number }, new MathAtomType[] { MathAtomType.Number } }, "x^{2}3");
      yield return ("x^{23}", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable }, new MathAtomType[] { MathAtomType.Number, MathAtomType.Number } }, "x^{23}");
      yield return ("x^2^3", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "x^{2}{}^{3}");
      yield return ("x^{2^3}", new MathAtomType[][] { new MathAtomType[] {MathAtomType.Variable},
        new MathAtomType[] { MathAtomType.Number },
      new MathAtomType[]{MathAtomType.Number} }, "x^{2^{3}}");
      yield return ("x^{^2*}", new MathAtomType[][] {
        new MathAtomType[]{MathAtomType.Variable },
        new MathAtomType[]{MathAtomType.Ordinary, MathAtomType.BinaryOperator },
        new MathAtomType[]{MathAtomType.Number } },
        "x^{{}^{2}*}");
      yield return ("^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^{2}");
      yield return ("{}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^{2}");
      yield return ("x^^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary }, new MathAtomType[] { } }, "x^{}{}^{2}");
      yield return ("5{x}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Number, MathAtomType.Variable }, new MathAtomType[] { } }, "5x^{2}");
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
      CheckAtomTypes(list, atomTypes[0]);

      IMathAtom firstAtom = list[0];
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
    private void CheckAtomTypes(IMathList list, MathAtomType[] types) {
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
      yield return (@"\left( 2 \right)^2", singletonList, 0, singletonNumber, @"(", @")", @"\left( 2\right) ^{2}");
      // Scripts on left
      yield return (@"\left(^2 \right )", singletonList, 0, new MathAtomType[] { MathAtomType.Ordinary }, @"(", @")", @"\left( {}^{2}\right) ");
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
  }
}
