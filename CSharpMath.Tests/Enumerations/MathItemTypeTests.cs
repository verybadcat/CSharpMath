using Xunit;
using CSharpMath.Enumerations;

namespace CSharpMath.Tests.Enumerations {
  public class MathItemTypeTests {
    [Fact]
    public void TestToText() {
      var str = MathItemType.BinaryOperator.ToText();
      Assert.Equal("Binary Operator", str);
    }
  }
}
