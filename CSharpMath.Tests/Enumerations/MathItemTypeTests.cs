using Xunit;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests.Enumerations {
  public class MathItemTypeTests {
    [Fact]
    public void TestToText() {
      var str = MathAtomType.BinaryOperator.ToText();
      Assert.Equal("Binary Operator", str);
    }
  }
}
