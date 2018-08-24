using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using CSharpMath.Display;
using CSharpMath.Tests.Resources;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<MathFont<char>, char> Create() {
      var boundsProvider = new TestGlyphBoundsProvider();
      return new TypesettingContext<MathFont<char>, char>(
        (font, size) => new MathFont<char>(size, font.Style),
        boundsProvider,
        new TestGlyphFinder(),
        new DoNothingFontChanger(),
        new Apple.JsonMathTable<MathFont<char>, char>(new TestFontMeasurer(), TestResources.LatinMath, new TestGlyphNameProvider(), boundsProvider)
    );
    }
  }
}
