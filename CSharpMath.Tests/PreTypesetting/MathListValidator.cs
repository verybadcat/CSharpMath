using CSharpMath.Atoms;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using Xunit;

namespace CSharpMath.Tests {
  internal static class MathListValidator {
    public static void CheckListContents(IMathList list) {
      Assert.Equal(10, list.Atoms.Count);
      var atom0 = list.Atoms[0];
      Assert.Equal(MathAtomType.UnaryOperator, atom0.AtomType);
      Assert.Equal("\u2212", atom0.Nucleus);
      Assert.Equal(new Range(0, 1), atom0.IndexRange);
      var atom1 = list.Atoms[1];
      Assert.Equal(MathAtomType.Number, atom1.AtomType);
      Assert.Equal("52", atom1.Nucleus);
      Assert.Equal(new Range(1, 2), atom1.IndexRange);
      var atom2 = list.Atoms[2];
      Assert.Equal(MathAtomType.Variable, atom2.AtomType);
      Assert.Equal("x", atom2.Nucleus);
      Assert.Equal(new Range(3, 1), atom2.IndexRange);
      var superScript = atom2.Superscript;
      Assert.Equal(3, superScript.Atoms.Count);

      var super0 = superScript.Atoms[0];
      Assert.Equal(MathAtomType.Number, super0.AtomType);
      Assert.Equal("13", super0.Nucleus);
      Assert.Equal(new Range(0, 2), super0.IndexRange);
      var super1 = superScript.Atoms[1];
      Assert.Equal(MathAtomType.BinaryOperator, super1.AtomType);
      Assert.Equal("+", super1.Nucleus);
      Assert.Equal(new Range(2, 1), super1.IndexRange);
      var super2 = superScript.Atoms[2];
      Assert.Equal(MathAtomType.Variable, super2.AtomType);
      Assert.Equal("y", super2.Nucleus);
      Assert.Equal(new Range(3, 1), super2.IndexRange);
    }
  }
}