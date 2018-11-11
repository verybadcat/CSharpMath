using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd {
  class TestGlyphNameProvider : CSharpMath.FrontEnd.IGlyphNameProvider<char> {
    TestGlyphNameProvider() { }
    public static TestGlyphNameProvider Instance { get; } = new TestGlyphNameProvider();

    public char GetGlyph(string glyphName) => glyphName.IsEmpty() ? (char)0 : glyphName[0];
    public string GetGlyphName(char glyph) => glyph.ToString();
  }
}
