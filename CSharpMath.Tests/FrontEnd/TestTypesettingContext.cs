using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Resources;
using CSharpMath.Display;
using CSharpMath.Tests.Resources;

namespace CSharpMath.Tests.FrontEnd {
  public static class TestTypesettingContexts {
    public static TypesettingContext<TestFont, char> Instance { get; } =
      new TypesettingContext<TestFont, char>(
        (font, size) => new TestFont(size),
        TestGlyphBoundsProvider.Instance,
        TestGlyphFinder.Instance,
        DoNothingFontChanger.Instance,
        new Apple.JsonMathTable<TestFont, char>(TestFontMeasurer.Instance, TestResources.LatinMath, TestGlyphNameProvider.Instance, TestGlyphBoundsProvider.Instance)
    );
  }
}
