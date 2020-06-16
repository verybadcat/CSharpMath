using System.Collections.Generic;
using NUnit.Framework;
namespace CSharpMath.Ios.Tests {
  [TestFixture]
  public class Tests {
    public Tests() {
    }
    [Test]
    [TestCaseSource(typeof(Rendering.Tests.TestRenderingMathData))]
    public void A(string file, string latex) {
      var v = IosMathLabels.MathView(latex, 50);
      Assert.NotNull(v);
    }
  }
}
