using Xunit;
using System.Linq;
namespace CSharpMath.CoreTests {
  using CSharpMath.Atom;
  using Atoms = CSharpMath.Atom.Atoms;
  public class LaTeXSettingsTests {
    [Fact]
    public void ForAsciiHandlesAllInputs() {
      for (char i = '\0'; i <= sbyte.MaxValue; i++)
        switch (i) {
          case '\\': // The command character is handled specially
          case '$': // Unimplemented
          case '#': // Unimplemented
          case '~': // Unimplemented
            Assert.DoesNotContain(LaTeXSettings.Commands, kvp => kvp.Key == i.ToString());
            break;
          default:
            Assert.Contains(LaTeXSettings.Commands, kvp => kvp.Key == i.ToString());
            break;
        }
    }
    [Fact]
    public void CommandForAtomIgnoresInnerLists() {
      var atom = new Atoms.Accent("\u0308", new MathList(new Atoms.Number("1")));
      atom.Superscript.Add(new Atoms.Number("4"));
      atom.Subscript.Add(new Atoms.Variable("x"));
      Assert.Equal(@"\ddot", LaTeXSettings.CommandForAtom(atom));
    }
    [Fact]
    public void AtomForCommandGeneratesACopy() {
      var atom = LaTeXSettings.AtomForCommand(@"\int");
      if (atom == null) throw new Xunit.Sdk.NotNullException();
      atom.IndexRange = Range.NotFound;
      var atom2 = LaTeXSettings.AtomForCommand(@"\int");
      if (atom2 == null) throw new Xunit.Sdk.NotNullException();
      Assert.Equal(Range.Zero, atom2.IndexRange);
    }
  }
}
