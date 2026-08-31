using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpMath.Atom;
using CSharpMath.Display;
using CSharpMath.Display.Displays;
using CSharpMath.Display.FrontEnd;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using BackEnd;
  public class TestCommandDisplay {
    public TestCommandDisplay() =>
      typefaces = Fonts.GlobalTypefaces.ToArray();
    readonly Typography.OpenFont.Typeface[] typefaces;
    public static IEnumerable<object[]> AllCommandValues =>
      Atom.LaTeXSettings.CommandSymbols.SecondToFirst.Keys
      .SelectMany(v => v.Nucleus.EnumerateRunes())
      .Distinct()
      .OrderBy(r => r.Value)
      .Select(rune => new object[] { rune });
    [Theory]
    [MemberData(nameof(AllCommandValues))]
    public void CommandsAreDisplayable(Rune ch) =>
      Assert.Contains(typefaces, font => font.GetGlyphIndex(ch.Value) != 0);

    [Fact]
    public void NotProportionalToOverlaysProportionalTo() {
      var fonts = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), 20);
      var negatedLine = Assert.Single(ParseLine(@"\not\propto", fonts).Displays);
      var baseLine = Assert.Single(ParseLine(@"\propto", fonts).Displays);
      Assert.IsType<TextLineDisplay<Fonts, Glyph>>(negatedLine);
      Assert.IsType<TextLineDisplay<Fonts, Glyph>>(baseLine);
      var negatedTextLine = (TextLineDisplay<Fonts, Glyph>)negatedLine;
      var baseTextLine = (TextLineDisplay<Fonts, Glyph>)baseLine;
      var negatedRun = Assert.Single(negatedTextLine.Runs);
      Assert.Single(baseTextLine.Runs);
      Assert.Equal(2, negatedRun.Run.Length);

      var expectedBase = GlyphFinder.Instance.Lookup(fonts, 0x221D);
      var expectedOverlay = GlyphFinder.Instance.Lookup(fonts, 0x0338);
      Assert.Equal(expectedBase.Info.GlyphIndex, negatedRun.Run.GlyphInfos[0].Glyph.Info.GlyphIndex);
      Assert.Same(expectedBase.Typeface, negatedRun.Run.GlyphInfos[0].Glyph.Typeface);
      Assert.Equal(expectedOverlay.Info.GlyphIndex, negatedRun.Run.GlyphInfos[1].Glyph.Info.GlyphIndex);
      Assert.Same(expectedOverlay.Typeface, negatedRun.Run.GlyphInfos[1].Glyph.Typeface);

      var advances = GlyphBoundsProvider.Instance.GetAdvancesForGlyphs(
        fonts, negatedRun.Run.Glyphs, negatedRun.Run.Length).Advances.ToArray();
      Assert.Equal(0, advances[1]);
      Assert.Equal(baseTextLine.Width, negatedTextLine.Width);

      var bounds = GlyphBoundsProvider.Instance.GetBoundingRectsForGlyphs(
        fonts, negatedRun.Run.Glyphs, negatedRun.Run.Length).ToArray();
      var baseInk = bounds[0];
      var overlayInk = bounds[1];
      overlayInk.Offset(advances[0], 0);
      Assert.True(overlayInk.IntersectsWith(baseInk));
    }

    static ListDisplay<Fonts, Glyph> ParseLine(string latex, Fonts fonts) {
      var result = Atom.LaTeXParser.MathListFromLaTeX(latex);
      Assert.Null(result.Error);
      return Assert.IsType<ListDisplay<Fonts, Glyph>>(
        Typesetter.CreateLine(result.Match(list => list, _ => throw new InvalidOperationException()),
          fonts, TypesettingContext.Instance, LineStyle.Display));
    }
  }
}
