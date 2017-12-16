using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display;
using Xunit;
using TGlyph = System.UInt16;


namespace CSharpMath.Tests {

  public class UnicodeFontChangerTests {
    private UnicodeFontChanger _createFontChanger() => new UnicodeFontChanger(new UnicodeGlyphFinder());

    [Theory]
    [Trait("Category", "Font")]
    [InlineData("m", FontStyle.Roman, "m")]
    [InlineData("m", FontStyle.Italic, "\uD835\uDC5A")]
    public void TestFontChanger(string input, FontStyle changeToFont, string expectedOutput) {
      var changer = _createFontChanger();

      var output = changer.ChangeFont(input, changeToFont);

      Assert.Equal(expectedOutput, output);
    }
  }
}
