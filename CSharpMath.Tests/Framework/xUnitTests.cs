using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests.Framework {
  public class XunitTests {
    [Fact]
    public void NullIsTypeTest() {
      Assert.IsNotType<object>(null);
    }
  }
}