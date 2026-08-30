using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Atom;
using CSharpMath.Display.Displays;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.Text;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Xunit;
using RenderingGlyph = CSharpMath.Rendering.BackEnd.Glyph;

namespace CSharpMath.Rendering.Tests {
  public class TestStyledLocalTypefaces {
    sealed class OneShotTypefaceEnumerable : IEnumerable<Typeface> {
      readonly IReadOnlyList<Typeface> _faces;
      public OneShotTypefaceEnumerable(IReadOnlyList<Typeface> faces) => _faces = faces;
      public int EnumerationCount { get; private set; }
      public IEnumerator<Typeface> GetEnumerator() {
        if (++EnumerationCount > 1) throw new InvalidOperationException("Local typefaces were enumerated more than once.");
        return _faces.GetEnumerator();
      }
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    static string FontPath(string fileName) => Path.GetFullPath(Path.Combine(
      TestRenderingFixture.ThisDirectory.FullName, "..", "Typography", "Demo", "Windows", "TestFonts", fileName));
    static Typeface Read(string fileName) {
      var path = FontPath(fileName);
      Assert.SkipUnless(File.Exists(path), "The redistributable Arimo fixtures are supplied by the Typography submodule.");
      using var stream = File.OpenRead(path);
      return new OpenFontReader().Read(stream) ?? throw new InvalidOperationException("Invalid font fixture.");
    }
    static RenderingGlyph GlyphFor(FontStyle style, params Typeface[] typefaces) => GlyphForText(style, "A", typefaces);
    static RenderingGlyph GlyphForText(FontStyle style, string text, params Typeface[] typefaces) {
      var input = new TextAtom.Style(new TextAtom.Text(text), style);
      var display = TextTypesetter.Layout(input, new Fonts(typefaces, 20), float.PositiveInfinity).relative;
      return ((TextRunDisplay<Fonts, RenderingGlyph>)display.Displays.Single()).Run.Glyphs.Single();
    }
    [Fact]
    public void SelectsEachOrdinaryStyleFromTheLocalFamily() {
      var regular = Read("Arimo-Regular.ttf");
      var bold = Read("Arimo-Bold.ttf");
      var italic = Read("Arimo-Italic.ttf");
      var boldItalic = Read("Arimo-BoldItalic.ttf");
      Assert.Same(regular, GlyphFor(FontStyle.Roman, regular, bold, italic, boldItalic).Typeface);
      Assert.Same(bold, GlyphFor(FontStyle.Bold, regular, bold, italic, boldItalic).Typeface);
      Assert.Same(italic, GlyphFor(FontStyle.Italic, regular, bold, italic, boldItalic).Typeface);
      Assert.Same(boldItalic, GlyphFor(FontStyle.BoldItalic, regular, bold, italic, boldItalic).Typeface);
    }
    [Fact]
    public void FallsBackToMathFontWhenStyleOrGlyphIsMissing() {
      var regular = Read("Arimo-Regular.ttf");
      var localFaces = new[] { regular, Read("Arimo-Bold.ttf"), Read("Arimo-Italic.ttf"), Read("Arimo-BoldItalic.ttf") };
      var bold = GlyphFor(FontStyle.Bold, regular);
      var expectedMath = Fonts.GlobalTypefaces.First(t => t.HasMathTable());
      var codepoint = Enumerable.Range(0x2200, 0xC00)
        .First(cp => localFaces.All(face => face.GetGlyphIndex(cp) == 0) && expectedMath.GetGlyphIndex(cp) != 0);
      var missingGlyph = GlyphForText(FontStyle.Roman, char.ConvertFromUtf32(codepoint), regular);
      Assert.Same(expectedMath, bold.Typeface);
      Assert.Same(expectedMath, missingGlyph.Typeface);
      Assert.Equal(expectedMath.GetGlyphIndex(codepoint), missingGlyph.Info.GlyphIndex);
    }
    [Fact]
    public void MixedStylesRetainTheirOwnMetrics() {
      var regular = Read("Arimo-Regular.ttf");
      var bold = Read("Arimo-Bold.ttf");
      var italic = Read("Arimo-Italic.ttf");
      var input = new TextAtom.List(new TextAtom[] {
        new TextAtom.Style(new TextAtom.Text("A"), FontStyle.Roman),
        new TextAtom.Style(new TextAtom.Text("A"), FontStyle.Bold),
        new TextAtom.Style(new TextAtom.Text("A"), FontStyle.Italic),
      });
      var display = TextTypesetter.Layout(input, new Fonts(new[] { regular, bold, italic }, 20), float.PositiveInfinity).relative;
      var runs = display.Displays.Cast<TextRunDisplay<Fonts, RenderingGlyph>>().ToArray();
      Assert.Equal(new[] { regular, bold, italic }, runs.Select(r => r.Run.Glyphs.Single().Typeface));
      Assert.All(runs, run => Assert.True(run.Ascent > 0));
      var expectedAdvances = new[] { regular, bold, italic }
        .Select(face => face.GetAdvanceWidthFromGlyphIndex(face.GetGlyphIndex('A'))
          * face.CalculateScaleToPixelFromPointSize(20)).ToArray();
      Assert.Equal(expectedAdvances.Sum(), display.Width, 3);
      Assert.Equal(0, runs[0].Position.X, 3);
      Assert.Equal(expectedAdvances[0], runs[1].Position.X, 3);
      Assert.Equal(expectedAdvances[0] + expectedAdvances[1], runs[2].Position.X, 3);
      Assert.Equal(runs[0].Position.Y, runs[1].Position.Y, 3);
      Assert.Equal(runs[1].Position.Y, runs[2].Position.Y, 3);
      Assert.Equal(runs[0].Ascent, runs[1].Ascent, 3);
      Assert.Equal(runs[1].Ascent, runs[2].Ascent, 3);
      Assert.Equal(runs[0].Descent, runs[1].Descent, 3);
      Assert.Equal(runs[1].Descent, runs[2].Descent, 3);
    }
    [Fact]
    public void SnapshotsOneShotLocalTypefaceCollectionsConsistently() {
      var regular = Read("Arimo-Regular.ttf");
      var bold = Read("Arimo-Bold.ttf");
      var source = new OneShotTypefaceEnumerable(new[] { regular, bold });
      var fonts = new Fonts(source, 20);
      Assert.Equal(1, source.EnumerationCount);
      var display = TextTypesetter.Layout(
        new TextAtom.Style(new TextAtom.Text("A"), FontStyle.Bold), fonts, float.PositiveInfinity).relative;
      var glyph = ((TextRunDisplay<Fonts, RenderingGlyph>)display.Displays.Single()).Run.Glyphs.Single();
      Assert.Same(bold, glyph.Typeface);
    }
  }
}
