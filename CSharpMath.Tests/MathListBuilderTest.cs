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
      new MathAtomType[]{MathAtomType.Number} }, "x^{2^{3 } }");
      yield return ("x^{^2*}", new MathAtomType[][] {
        new MathAtomType[]{MathAtomType.Variable },
        new MathAtomType[]{MathAtomType.Ordinary, MathAtomType.BinaryOperator },
        new MathAtomType[]{MathAtomType.Number } },
        "x^{{}^{2}*}");
      yield return ("^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^{2}");
      yield return ("{}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Ordinary }, new MathAtomType[] { MathAtomType.Number } }, "{}^{2}");
      yield return ("x^^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Variable, MathAtomType.Ordinary } , new MathAtomType[] { } }, "x^{}{}^{2}");
      yield return ("5{x}^2", new MathAtomType[][] { new MathAtomType[] { MathAtomType.Number, MathAtomType.Variable }, new MathAtomType[] { } }, "5x^2");
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

      // TODO: convert back to string and check.
    }

    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(IMathList list, MathAtomType[] types) {
      int atomCount = (list == null) ? 0 : list.Atoms.Count;
      Assert.Equal(types.Count(), atomCount);
      for (int i=0; i<atomCount; i++) {
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

      //TODO: convert back and check.
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

      // TODO: convert back and check
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

      // TODO: convert back and check
    }
  }
}
