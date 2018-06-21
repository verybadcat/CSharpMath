using CSharpMath.Display;
using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd
{
  class TestGlyphFinder : IGlyphFinder<MathFont<char>, char> {
    public char FindGlyphForCharacterAtIndex(MathFont<char> font, int index, string str) {
      return str[index];
    }

    public char[] FindGlyphs(MathFont<char> font, string str) => str.ToArray();

    public string FindStringDebugPurposesOnly(char[] glyphs) => new string(glyphs);
    public bool GlyphIsEmpty(char glyph)
      => glyph == 0;
  }
}
