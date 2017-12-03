using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<char> Create()
      => new TypesettingContext<char>(
        new TestFontMeasurer(),
        new TestGlyphBoundsProvider(),
        new TestGlyphFinder(),
        ResourceLoader.LatinMath);
  }
}
