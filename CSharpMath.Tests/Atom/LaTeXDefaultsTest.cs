using Xunit;
using System.Linq;
namespace CSharpMath.Tests.Atom {
  using CSharpMath.Atom;
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
  }
}
