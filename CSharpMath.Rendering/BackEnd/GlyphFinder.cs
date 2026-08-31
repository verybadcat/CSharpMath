using System;
using System.Collections.Generic;
using System.Linq;
using CSharpMath.Atom;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace CSharpMath.Rendering.BackEnd {
  public class GlyphFinder : Display.FrontEnd.IGlyphFinder<Fonts, Glyph> {
    private GlyphFinder() { }
    //http://unicode.org/charts/PDF/U25A0.pdf
    //U+25A1 WHITE SQUARE may be used to represent a missing ideograph
    //The glyph of this character is in the Latin Modern Math font
    public const char GlyphNotFound = '□';
    public static GlyphFinder Instance { get; } = new GlyphFinder();
    public Glyph Lookup(Fonts fonts, int codepoint) {
      return Lookup(fonts.GetTypefacesSnapshot(), codepoint);
    }
    static Glyph Lookup(IEnumerable<Typeface> typefaces, int codepoint) {
      foreach (var font in typefaces) {
        var g = font.GetGlyphIndex(codepoint);
        if (g != 0) return new Glyph(font, font.GetGlyph(g));
      }
      return Lookup(typefaces, GlyphNotFound);
    }
    static bool IsOrdinary(FontStyle style) =>
      style is FontStyle.Roman or FontStyle.Bold or FontStyle.Italic or FontStyle.BoldItalic;
    static FontStyle? GetOrdinaryStyle(Typeface typeface) {
      var translated = typeface.TranslateOS2FontStyle();
      var bold = (translated & TranslatedOS2FontStyle.BOLD) != 0;
      var italic = (translated & (TranslatedOS2FontStyle.ITALIC | TranslatedOS2FontStyle.OBLIQUE)) != 0;
      return bold && italic ? FontStyle.BoldItalic
        : bold ? FontStyle.Bold
        : italic ? FontStyle.Italic
        : FontStyle.Roman;
    }
    internal static IReadOnlyDictionary<FontStyle, IReadOnlyList<Typeface>> BuildLocalStyleLookup(
      IReadOnlyList<Typeface> localTypefaces) {
      var result = new Dictionary<FontStyle, IReadOnlyList<Typeface>>();
      var byStyle = new Dictionary<FontStyle, List<Typeface>>();
      foreach (var family in localTypefaces.GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)) {
        foreach (var face in family) {
          var style = GetOrdinaryStyle(face).GetValueOrDefault();
          if (!byStyle.TryGetValue(style, out var faces))
            byStyle[style] = faces = new List<Typeface>();
          faces.Add(face);
        }
      }
      foreach (var pair in byStyle)
        result[pair.Key] = pair.Value;
      return result;
    }
    Glyph LookupLocalStyle(IReadOnlyDictionary<FontStyle, IReadOnlyList<Typeface>> localStyles, int codepoint, FontStyle style) {
      if (!localStyles.TryGetValue(style, out var faces)) return Glyph.Empty;
      foreach (var face in faces) {
        var glyph = face.GetGlyphIndex(codepoint);
        if (glyph != 0) return new Glyph(face, face.GetGlyph(glyph));
      }
      return Glyph.Empty;
    }
    /// <summary>Find ordinary text glyphs in a matching local family before applying mathematical Unicode styling.</summary>
    internal System.Collections.Generic.IEnumerable<Glyph> FindGlyphs(Fonts fonts, string str, FontStyle style) {
      var snapshot = fonts.CaptureSnapshot();
      var localStyles = BuildLocalStyleLookup(snapshot.LocalTypefaces);
      var styled = Display.UnicodeFontChanger.ChangeFont(str, style);
      var sourceCodepoints = Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray()).ToArray();
      var styledCodepoints = Typography.OpenFont.StringUtils.GetCodepoints(styled.ToCharArray()).ToArray();
      for (var i = 0; i < sourceCodepoints.Length; i++) {
        var local = IsOrdinary(style) ? LookupLocalStyle(localStyles, sourceCodepoints[i], style) : Glyph.Empty;
        yield return local.IsEmpty ? Lookup(snapshot.Typefaces, styledCodepoints[i]) : local;
      }
    }
    internal System.Collections.Generic.IEnumerable<Glyph> FindGlyphs(Fonts fonts, string str, TextStyle semanticStyle) {
      var snapshot = fonts.CaptureSnapshot();
      var descriptors = snapshot.Descriptors;
      var localTypefaces = snapshot.LocalTypefaces;
      var typefaces = snapshot.Typefaces;
      var localStyles = BuildLocalStyleLookup(localTypefaces);
      // Slanted is never approximated with mathematical italic. If no slanted face exists,
      // retry the same family/weight as upright before using the legacy Unicode fallback.
      var fallbackSemanticStyle = semanticStyle.Posture == FontPosture.Slanted
        ? semanticStyle.WithPosture(FontPosture.Upright) : semanticStyle;
      var legacyStyle = fallbackSemanticStyle.ToFontStyle();
      var styled = Display.UnicodeFontChanger.ChangeFont(str, legacyStyle);
      var sourceCodepoints = Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray()).ToArray();
      var styledCodepoints = Typography.OpenFont.StringUtils.GetCodepoints(styled.ToCharArray()).ToArray();
      for (var i = 0; i < sourceCodepoints.Length; i++) {
        var found = false;
        var matchingDescriptors = snapshot.LocalDescriptors.Where(d => Matches(d, semanticStyle));
        if (semanticStyle.Posture == FontPosture.Slanted)
          matchingDescriptors = matchingDescriptors.Concat(
            snapshot.LocalDescriptors.Where(d => Matches(d, fallbackSemanticStyle)));
        matchingDescriptors = matchingDescriptors.Concat(
          snapshot.LocalDescriptors.Where(d => d.Family == styleFamily(semanticStyle) &&
            d.Weight == semanticStyle.Weight &&
            (semanticStyle.Posture != FontPosture.Slanted || d.Posture != FontPosture.Italic)));
        matchingDescriptors = matchingDescriptors.Concat(snapshot.LocalDescriptors
          .Where(d => semanticStyle.Posture != FontPosture.Slanted || d.Posture != FontPosture.Italic));
        foreach (var descriptor in matchingDescriptors) {
          var face = semanticStyle.Capitals == FontCapitals.SmallCapitals
            ? descriptor.SmallCapitalsTypeface ?? descriptor.Typeface : descriptor.Typeface;
          if (semanticStyle.Capitals == FontCapitals.SmallCapitals &&
              descriptor.SmallCapitalsGlyphMap.TryGetValue(sourceCodepoints[i], out var mapped)) {
            yield return new Glyph(face, face.GetGlyph(mapped));
            found = true;
            break;
          }
          var glyphIndex = face.GetGlyphIndex(sourceCodepoints[i]);
          if (glyphIndex != 0) {
            yield return new Glyph(face, face.GetGlyph(glyphIndex));
            found = true;
            break;
          }
        }
        if (!found) {
          foreach (var descriptor in descriptors.Skip(snapshot.LocalDescriptors.Length)
            .Where(d => Matches(d, semanticStyle) || Matches(d, fallbackSemanticStyle))) {
            var face = semanticStyle.Capitals == FontCapitals.SmallCapitals
              ? descriptor.SmallCapitalsTypeface ?? descriptor.Typeface : descriptor.Typeface;
            var glyphIndex = face.GetGlyphIndex(sourceCodepoints[i]);
            if (glyphIndex != 0) {
              yield return new Glyph(face, face.GetGlyph(glyphIndex));
              found = true;
              break;
            }
          }
        }
        if (!found) {
          var local = IsOrdinary(legacyStyle) ? LookupLocalStyle(localStyles, sourceCodepoints[i], legacyStyle) : Glyph.Empty;
          yield return local.IsEmpty ? Lookup(typefaces, styledCodepoints[i]) : local;
        }
      }
      static FontFamily styleFamily(TextStyle style) => style.Family == FontFamily.Default
        ? FontFamily.Roman : style.Family;
    }
    static bool Matches(TypefaceDescriptor descriptor, TextStyle style) =>
      (descriptor.Family == style.Family ||
       descriptor.Family == FontFamily.Default && style.Family == FontFamily.Roman ||
       descriptor.Family == FontFamily.Roman && style.Family == FontFamily.Default) &&
      descriptor.Weight == style.Weight && descriptor.Posture == style.Posture &&
      (style.Capitals == FontCapitals.Normal || descriptor.SmallCapitalsTypeface != null || descriptor.SmallCapitalsGlyphMap.Count != 0);
    public int GetCodepoint(string str, int index) =>
      index + 1 < str.Length
      && char.IsHighSurrogate(str[index])
      && char.IsLowSurrogate(str[index + 1])
      ? char.ConvertToUtf32(str[index], str[index + 1])
      : index > 0
      && char.IsHighSurrogate(str[index - 1])
      && char.IsLowSurrogate(str[index])
      ? char.ConvertToUtf32(str[index - 1], str[index])
      : str[index];
    public Glyph FindGlyphForCharacterAtIndex(Fonts fonts, int index, string str) =>
      Lookup(fonts, GetCodepoint(str, index));
    public System.Collections.Generic.IEnumerable<Glyph> FindGlyphs(Fonts fonts, string str) {
      var typefaces = fonts.GetTypefacesSnapshot();
      return Typography.OpenFont.StringUtils.GetCodepoints(str.ToCharArray())
        .Select(c => Lookup(typefaces, c));
    }
    public bool GlyphIsEmpty(Glyph glyph) => glyph.IsEmpty;
    public Glyph EmptyGlyph => Glyph.Empty;
  }
}
