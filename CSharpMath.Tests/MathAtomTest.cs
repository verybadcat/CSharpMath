using CSharpMath.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class MathAtomTest {
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

  }
}
