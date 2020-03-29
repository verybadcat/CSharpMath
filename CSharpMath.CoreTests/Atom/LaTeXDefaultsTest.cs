using Xunit;
using System.Linq;
namespace CSharpMath.CoreTests.Atom {
  using CSharpMath.Atom;
  using Atoms = CSharpMath.Atom.Atoms;
  public class LaTeXDefaultsTest {
    [Fact]
    public void ForAsciiHandlesAllInputs() {
      for (sbyte i = -36 + 1; i != -36; i++) // Break loop at arbitrary negative value (-36)
        switch (i) {
          case var _ when i < 0:
            Assert.Throws<System.ArgumentOutOfRangeException>(
              () => LaTeXDefaults.ForAscii(i)
            );
            break;
          case var _ when i <= ' ':
          case (sbyte)'\u007F':
          case (sbyte)'$':
          case (sbyte)'%':
          case (sbyte)'#':
          case (sbyte)'&':
          case (sbyte)'~':
          case (sbyte)'\'':
          case (sbyte)'^': 
          case (sbyte)'_':
          case (sbyte)'{':
          case (sbyte)'}':
          case (sbyte)'\\':
            Assert.Null(LaTeXDefaults.ForAscii(i));
            break;
          default:
            Assert.NotNull(LaTeXDefaults.ForAscii(i));
            break;
        }
    }
    [Fact]
    public void CommandForAtomIgnoresInnerLists() {
      var atom = new Atoms.Accent("\u0308", new MathList(new Atoms.Number("1")));
      atom.Superscript = new MathList(new Atoms.Number("4"));
      atom.Subscript = new MathList(new Atoms.Variable("x"));
      Assert.Equal("ddot", LaTeXDefaults.CommandForAtom(atom));
    }
    [Fact]
    public void AtomForCommandGeneratesACopy() {
      var atom = LaTeXDefaults.AtomForCommand("int");
      atom.IndexRange = Range.NotFound;
      Assert.Equal(Range.Zero, LaTeXDefaults.AtomForCommand("int").IndexRange);
    }
  }
}
