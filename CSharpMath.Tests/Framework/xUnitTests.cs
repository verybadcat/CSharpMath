using Xunit;

namespace CSharpMath.Tests.Framework {
  public class XunitTests {
    [Fact]
    public void NullIsTypeTest() {
      Assert.IsNotType<object>(null);
    }
  }
}