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

      string errorDescription = "Error for string: " + input;
      CheckAtomTypes(list, atomTypes, errorDescription);

      // TODO: convert back and check.
    }

    private void CheckAtomTypes(IMathList list, MathAtomType[] types, string errorDescription) {
      for (int i=0; i<list.Atoms.Count; i++) {
        var atom = list.Atoms[i];
        Assert.NotNull(atom);
        Assert.Equal(atom.AtomType, types[i]);
      }
    }
  }
}
