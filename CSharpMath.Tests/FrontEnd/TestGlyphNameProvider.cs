using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd {
  class TestGlyphNameProvider : IGlyphNameProvider<char> {
    public char GetGlyph(string glyphName) => throw new NotImplementedException();
    public string GetGlyphName(char glyph) => throw new NotImplementedException();
  }
}
