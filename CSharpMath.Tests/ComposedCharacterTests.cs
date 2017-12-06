using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Tests {
  public class ComposedCharacterTests {
    [Fact]
    public void TestCharacterRanges() {
      string foo = "\u0104\u0301Hello\u0104\u0304  world";
      int length = foo.Length;
      var unicode = new UnicodeEncoding();
      byte[] encodeFoo = unicode.GetBytes(foo);
      int[] fooInfo = StringInfo.ParseCombiningCharacters(foo);
      Assert.DoesNotContain(1, fooInfo);
      Assert.DoesNotContain(8, fooInfo);
    }
  }
}
