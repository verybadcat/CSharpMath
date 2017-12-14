using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd
{
  class TestGlyphFinder : IGlyphFinder<char> {
    public char FindGlyphForCharacterAtIndex(int index, string str) {
      return str[index];
    }

    public char[] FindGlyphs(string str) => str.ToArray();

    public string FindStringDebugPurposesOnly(char[] glyphs) => new string(glyphs);
  }
}
