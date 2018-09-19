using CSharpMath.Display;
using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd
{
  class TestGlyphFinder : IGlyphFinder<TestMathFont, char> {
    public char FindGlyphForCharacterAtIndex(TestMathFont font, int index, string str) {
      return str[index];
    }

    public IEnumerable<char> FindGlyphs(TestMathFont font, string str) => str;

    public string FindStringDebugPurposesOnly(char[] glyphs) => new string(glyphs);
    public bool GlyphIsEmpty(char glyph)
      => glyph is '\0';
    public char EmptyGlyph => '\0';
  }
}
