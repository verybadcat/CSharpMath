using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpMath.Atom;
using CSharpMath.Display.Displays;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.Text;
using Typography.OpenFont;
using Xunit;
using BackendGlyph = CSharpMath.Rendering.BackEnd.Glyph;

namespace CSharpMath.Rendering.Text.Tests {
  public class SemanticTypefaceDescriptorTests {
    static Typeface Read(string name) {
      var path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "Typography", "Demo", "Windows", "TestFonts", name));
      Assert.True(File.Exists(path), path);
      using var stream = File.OpenRead(path);
      return new OpenFontReader().Read(stream);
    }

    [Fact]
    public void ConfiguredSmallCapMapUsesGlyphAndRetainsSourceText() {
      var source = Read("SourceSerifPro-Regular.otf");
      var descriptor = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular,
        FontPosture.Upright, smallCapitalsGlyphMap: new System.Collections.Generic.Dictionary<int, ushort> {
          ['a'] = 1108
        });
      var fonts = Fonts.FromDescriptors(new[] { descriptor }, 20);
      var atom = new TextAtom.Style(new TextAtom.Text("a"), new TextStyleChange(
        FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.SmallCapitals));
      var line = TextTypesetter.Layout(atom, fonts, float.PositiveInfinity).relative;
      var run = Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single());
      Assert.Equal("a", run.Run.Text.ToString());
      Assert.Equal((ushort)1108, run.Run.GlyphInfos.Single().Glyph.Info.GlyphIndex);
      Assert.Same(source, run.Run.GlyphInfos.Single().Glyph.Typeface);
      Assert.Equal(589, source.GetAdvanceWidthFromGlyphIndex(1108));
      Assert.Equal(664, source.GetAdvanceWidthFromGlyphIndex(source.GetGlyphIndex('\u0041')));
      Assert.Equal(509, source.GetAdvanceWidthFromGlyphIndex(source.GetGlyphIndex('\u0061')));
      Assert.Equal(source.GetAdvanceWidthFromGlyphIndex(1108),
        run.Run.GlyphInfos.Single().Glyph.Typeface.GetAdvanceWidthFromGlyphIndex(1108));
    }

    [Fact]
    public void DescriptorPreservesSemanticIdentityAndStableEquality() {
      var source = Read("SourceSerifPro-Regular.otf");
      var italic = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular, FontPosture.Italic,
        supportedFeatures: new[] { "kern", "liga", "kern" });
      var slanted = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular, FontPosture.Slanted,
        supportedFeatures: new[] { "liga", "kern" });
      var same = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular, FontPosture.Italic,
        supportedFeatures: new[] { "liga", "kern" });
      Assert.NotEqual(italic, slanted);
      Assert.Equal(italic, same);
      Assert.Equal(italic.GetHashCode(), same.GetHashCode());
      Assert.Equal(new[] { "kern", "liga" }, italic.SupportedFeatures);
      Assert.Equal(FontFamily.Roman, italic.Family);
      Assert.Equal(FontWeight.Regular, italic.Weight);
      Assert.Equal(FontPosture.Italic, italic.Posture);
    }

    [Fact]
    public void DescriptorCollectionsAreDefensiveAndDefaultStyleRemainsCompatible() {
      var source = Read("SourceSerifPro-Regular.otf");
      var map = new Dictionary<int, ushort> { ['a'] = 1108 };
      var features = new List<string> { "kern" };
      var descriptor = new TypefaceDescriptor(source, FontFamily.Default, FontWeight.Regular,
        FontPosture.Upright, smallCapitalsGlyphMap: map, supportedFeatures: features);
      map['a'] = 1109;
      features.Add("liga");
      Assert.Equal((ushort)1108, descriptor.SmallCapitalsGlyphMap['a']);
      Assert.Single(descriptor.SupportedFeatures);
      Assert.Throws<NotSupportedException>(() => ((ICollection<string>)descriptor.SupportedFeatures).Add("liga"));
      Assert.Throws<ArgumentNullException>(() => Fonts.FromDescriptors(null!, 20));
      Assert.Throws<ArgumentException>(() => Fonts.FromDescriptors(new TypefaceDescriptor[] { null! }, 20));
    }

    [Fact]
    public void AbsoluteAndRelativeSizeRetainDescriptorGlyphSelection() {
      var source = Read("SourceSerifPro-Regular.otf");
      var descriptor = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular,
        FontPosture.Upright, smallCapitalsGlyphMap: new Dictionary<int, ushort> { ['a'] = 1108 });
      var fonts = Fonts.FromDescriptors(new[] { descriptor }, 20);
      static TextRunDisplay<Fonts, BackendGlyph> Run(TextAtom atom, Fonts fonts) {
        var line = TextTypesetter.Layout(atom, fonts, float.PositiveInfinity).relative;
        return Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single());
      }
      var normal = Run(new TextAtom.Style(new TextAtom.Text("a"), new TextStyleChange(
        FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.SmallCapitals)), fonts);
      var absolute = Run(new TextAtom.Size(new TextAtom.Style(new TextAtom.Text("a"), new TextStyleChange(
        FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.SmallCapitals)), 40), fonts);
      var relative = Run(new TextAtom.RelativeSize(new TextAtom.Style(new TextAtom.Text("a"), new TextStyleChange(
        FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.SmallCapitals)), "large"), fonts);
      Assert.All(new[] { normal, absolute, relative }, run => {
        Assert.Same(source, run.Run.GlyphInfos.Single().Glyph.Typeface);
        Assert.Equal((ushort)1108, run.Run.GlyphInfos.Single().Glyph.Info.GlyphIndex);
      });
      Assert.True(absolute.Width > normal.Width);
      Assert.True(relative.Width > normal.Width);
    }

    [Fact]
    public void MutableDescriptorCollectionIsObservedBetweenSnapshots() {
      var source = Read("SourceSerifPro-Regular.otf");
      var descriptors = new List<TypefaceDescriptor> {
        new(source, FontFamily.Roman, FontWeight.Regular, FontPosture.Upright)
      };
      var fonts = Fonts.FromDescriptors(descriptors, 20);
      var globalCount = Fonts.GlobalTypefaces.Count();
      Assert.Equal(globalCount + 1, fonts.Typefaces.Count());
      descriptors.Add(new TypefaceDescriptor(source, FontFamily.SansSerif, FontWeight.Bold, FontPosture.Italic));
      Assert.Equal(globalCount + 2, fonts.Typefaces.Count());
      descriptors.RemoveAt(1);
      Assert.Equal(globalCount + 1, fonts.Typefaces.Count());
    }

    [Fact]
    public void UnicodeSourceTextKeepsScalarsAndClusterStarts() {
      var source = Read("SourceSerifPro-Regular.otf");
      var fonts = Fonts.FromDescriptors(new[] { new TypefaceDescriptor(source, FontFamily.Roman,
        FontWeight.Regular, FontPosture.Upright) }, 20);
      var text = "a,\u00E9\U0001F984";
      var line = TextTypesetter.Layout(new TextAtom.Text(text), fonts, float.PositiveInfinity).relative;
      var run = Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single());
      Assert.Equal(text, run.Run.Text.ToString());
      Assert.Equal(4, run.Run.GlyphInfos.Count());
    }

    [Fact]
    public void OneShotDescriptorEnumerableIsEnumeratedOncePerLayout() {
      var source = Read("SourceSerifPro-Regular.otf");
      var descriptor = new TypefaceDescriptor(source, FontFamily.Roman, FontWeight.Regular, FontPosture.Upright);
      var oneShot = new OneShotDescriptors(descriptor);
      var fonts = Fonts.FromDescriptors(oneShot, 20);
      var atom = new TextAtom.Style(new TextAtom.Text("A"), new TextStyleChange(
        FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal));
      var line = TextTypesetter.Layout(atom, fonts, float.PositiveInfinity).relative;
      var run = Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single());
      Assert.Same(source, run.Run.GlyphInfos.Single().Glyph.Typeface);
      Assert.Equal(1, oneShot.EnumerationCount);
    }

    [Fact]
    public void ExplicitSlantedFaceWinsOverEarlierItalicAndFallsBackToUpright() {
      var upright = Read("SourceSerifPro-Regular.otf");
      var italic = Read("SourceSansPro-Regular.otf");
      var slanted = Read("ChulabhornLikitText-Regular.otf");
      var descriptors = new List<TypefaceDescriptor> {
        new(upright, FontFamily.Roman, FontWeight.Regular, FontPosture.Upright),
        new(italic, FontFamily.Roman, FontWeight.Regular, FontPosture.Italic),
        new(slanted, FontFamily.Roman, FontWeight.Regular, FontPosture.Slanted)
      };
      var fonts = Fonts.FromDescriptors(descriptors, 20);
      static Typeface Selected(Fonts fonts, FontPosture posture) {
        var atom = new TextAtom.Style(new TextAtom.Text("A"), new TextStyleChange(
          FontFamily.Roman, FontWeight.Regular, posture, FontCapitals.Normal));
        var line = TextTypesetter.Layout(atom, fonts, float.PositiveInfinity).relative;
        return Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single()).Run.GlyphInfos.Single().Glyph.Typeface;
      }
      Assert.Same(slanted, Selected(fonts, FontPosture.Slanted));
      descriptors.RemoveAt(2);
      Assert.Same(upright, Selected(fonts, FontPosture.Slanted));
    }

    [Fact]
    public void LocalNonExactFamilyDescriptorBeatsGlobalExactCandidate() {
      var local = Read("SourceSansPro-Regular.otf");
      var fonts = Fonts.FromDescriptors(new[] {
        new TypefaceDescriptor(local, FontFamily.SansSerif, FontWeight.Regular, FontPosture.Upright)
      }, 20);
      var atom = new TextAtom.Style(new TextAtom.Text("A"), new TextStyleChange(
        FontFamily.SansSerif, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal));
      var line = TextTypesetter.Layout(atom, fonts, float.PositiveInfinity).relative;
      var run = Assert.IsType<TextRunDisplay<Fonts, BackendGlyph>>(line.Displays.Single());
      Assert.Same(local, run.Run.GlyphInfos.Single().Glyph.Typeface);
    }

    sealed class OneShotDescriptors : IEnumerable<TypefaceDescriptor> {
      readonly TypefaceDescriptor descriptor;
      public int EnumerationCount { get; private set; }
      public OneShotDescriptors(TypefaceDescriptor descriptor) => this.descriptor = descriptor;
      public IEnumerator<TypefaceDescriptor> GetEnumerator() {
        if (++EnumerationCount != 1) throw new InvalidOperationException("descriptor source enumerated twice");
        yield return descriptor;
      }
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void DescriptorRejectsInvalidGlyphMapAndFeatures() {
      var source = Read("SourceSerifPro-Regular.otf");
      Assert.Throws<System.ArgumentOutOfRangeException>(() => new TypefaceDescriptor(
        source, FontFamily.Roman, FontWeight.Regular, FontPosture.Upright,
        smallCapitalsGlyphMap: new System.Collections.Generic.Dictionary<int, ushort> { ['a'] = 0 }));
      Assert.Throws<System.ArgumentException>(() => new TypefaceDescriptor(
        source, FontFamily.Roman, FontWeight.Regular, FontPosture.Upright,
        supportedFeatures: new[] { "bad" }));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TypefaceDescriptor(
        source, (FontFamily)99, FontWeight.Regular, FontPosture.Upright));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TypefaceDescriptor(
        source, FontFamily.Roman, (FontWeight)99, FontPosture.Upright));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TypefaceDescriptor(
        source, FontFamily.Roman, FontWeight.Regular, (FontPosture)99));
    }
  }
}
