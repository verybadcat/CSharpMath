using Xunit;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests.PreTypesetting {
  public class MathItemTypeTests {
    [Fact]
    public void TestToText() =>
      Assert.Equal("Binary Operator", MathAtomType.BinaryOperator.ToText());
  }
}
