using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath.Display.Text;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<MathFont<char>, char> Create()
      => new TypesettingContext<MathFont<char>, char>(
        new TestFontMeasurer(),
        (font, size) => new MathFont<char>(size, font.Style),
        new TestGlyphBoundsProvider(),
        new TestGlyphNameProvider(),
        new TestGlyphFinder(),
        ResourceLoader.LatinMath);
  }
}
