using CSharpMath.Rendering.FrontEnd;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  public class TestConstrainedTextLayout {
    [Theory]
    [InlineData(880, 37, 113, 730)]
    [InlineData(880, 0, 0, 880)]
    public void ContentWidthSubtractsAsymmetricPadding(double available, double left, double right, float expected) =>
      Assert.Equal(expected, ConstrainedTextLayout.ContentWidth(available, left, right));

    [Fact]
    public void ContentWidthPreservesUnboundedConstraints() {
      Assert.True(float.IsPositiveInfinity(ConstrainedTextLayout.ContentWidth(double.PositiveInfinity, 10, 20)));
      Assert.True(float.IsNaN(ConstrainedTextLayout.ContentWidth(double.NaN, 10, 20)));
    }
  }
}
