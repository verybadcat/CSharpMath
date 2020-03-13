using System.Globalization;
using System.Text;
using Xunit;

namespace CSharpMath.Tests.Framework {
  public class UnicodeEncodingTest {
    [Fact]
    public void TestCharacterRanges() {
      string foo = "\u0104\u0301Hello\u0104\u0304  world";
      Assert.Equal(16, foo.Length);
      Assert.Equal(2 * foo.Length, new UnicodeEncoding().GetBytes(foo).Length);
      int[] fooInfo = StringInfo.ParseCombiningCharacters(foo);
      Assert.DoesNotContain(1, fooInfo);
      Assert.DoesNotContain(8, fooInfo);
    }
  }
}