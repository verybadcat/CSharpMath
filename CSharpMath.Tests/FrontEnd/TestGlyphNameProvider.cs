using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd {
  class TestGlyphNameProvider : CSharpMath.FrontEnd.IGlyphNameProvider<char> {
    public char GetGlyph(string glyphName) => glyphName.IsEmpty() ? (char)0 : glyphName[0];
    public string GetGlyphName(char glyph) => glyph.ToString();
  }
}
