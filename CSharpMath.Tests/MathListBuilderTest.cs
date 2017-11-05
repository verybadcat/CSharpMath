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

      // TODO: convert back and check.
    }

    [Theory, MemberData(nameof(SuperscriptTestData))]
    public void TestSuperscript(string input, MathAtomType[][] atomTypes, string output) {
      var builder = new MathListBuilder(input);
      var list = builder.Build();
      Assert.Null(builder.Error);
      CheckAtomTypes(list, atomTypes[0]);

      IMathAtom firstAtom = list.Atoms[0];
      var types = atomTypes[1];
      if (types.Count() > 0) {
        Assert.NotNull(firstAtom.Superscript);
      }
      var superList = firstAtom.Superscript;
      CheckAtomTypes(superList, atomTypes[1]);
      if (atomTypes.Count() == 3) {
        // one more level
        var superFirst = superList.Atoms[0];
        var superSuperList = superFirst.Superscript;
        CheckAtomTypes(superSuperList, atomTypes[2]);
      }

      // TODO: convert back to string and check.
    }

    /// <summary>Safe to call with a null list. Types cannot be null however.</summary>
    private void CheckAtomTypes(IMathList list, MathAtomType[] types) {
      int atomCount = (list == null) ? 0 : list.Atoms.Count;
      Assert.Equal(types.Count(), atomCount);
      for (int i=0; i<atomCount; i++) {
        var atom = list.Atoms[i];
        Assert.NotNull(atom);
        Assert.Equal(atom.AtomType, types[i]);
      }
    }
  }
}
