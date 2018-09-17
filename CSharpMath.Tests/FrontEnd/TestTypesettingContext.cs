using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using CSharpMath.Display;
using CSharpMath.Tests.Resources;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<TestMathFont, char> Create() {
      var boundsProvider = new TestGlyphBoundsProvider();
      return new TypesettingContext<TestMathFont, char>(
        (font, size) => new TestMathFont(size),
        boundsProvider,
        new TestGlyphFinder(),
        new DoNothingFontChanger(),
        new Apple.JsonMathTable<TestMathFont, char>(new TestFontMeasurer(), TestResources.LatinMath, new TestGlyphNameProvider(), boundsProvider)
    );
    }
  }
}
