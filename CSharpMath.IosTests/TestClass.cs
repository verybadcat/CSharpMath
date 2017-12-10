using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;

namespace CSharpMath.IosTests {
  [TestFixture]
  public class TestClass {
    [Test]
    public void AAATestHello() {
      // TODO: Add your test code here
      var hello = "Hello";
      var NsHello = new NSString(hello);
      var hello2 = NsHello.ToString();
      Assert.AreEqual(hello, hello2);
    }
  }
}
