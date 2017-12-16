using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using CSharpMath.Display;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<MathFont<char>, char> Create()
      => new TypesettingContext<MathFont<char>, char>(
        new TestFontMeasurer(),
        (font, size) => new MathFont<char>(size, font.Style),
        new TestGlyphBoundsProvider(),
        new TestGlyphNameProvider(),
        new TestGlyphFinder(),
      new DoNothingFontChanger(),
        ResourceLoader.LatinMath

    );
  }
}
