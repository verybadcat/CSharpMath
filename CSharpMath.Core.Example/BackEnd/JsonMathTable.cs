using System.Collections.Generic;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Display.FrontEnd;
using Newtonsoft.Json.Linq;

namespace CSharpMath.Core.BackEnd {
  using TFont = TestFont;
  using TGlyph = System.Text.Rune;
  /// <summary>Holds lots of constants for spacing between
  /// various visible elements by reading a JSON file.</summary>
  public class JsonMathTable : FontMathTable<TFont, TGlyph> {
    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    private readonly JToken _mathTable;
    private readonly JObject _constantsDictionary;
    private readonly JObject _vAssemblyTable, _hAssemblyTable;
    private readonly JObject _italicTable;
    public TestFontMeasurer FontMeasurer { get; }
    public TestGlyphNameProvider GlyphNameProvider { get; }
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; }
    protected float FontUnitsToPt(TFont font, int fontUnits) =>
      fontUnits * font.PointSize / FontMeasurer.GetUnitsPerEm(font);
    private float ConstantFromTable(TFont font, string constantName) =>
      FontUnitsToPt(font, _constantsDictionary[constantName]!.Value<int>());
    private float PercentFromTable(string name) =>
      // different from _ConstantFromTable in that the _ConstantFromTable requires
      // a font and uses _FontUnitsToPt, while this is just a straight percentage.
      _constantsDictionary[name]!.Value<int>() / 100f;
    public override float RadicalDisplayStyleVerticalGap(TFont font) =>
      ConstantFromTable(font, nameof(RadicalDisplayStyleVerticalGap));
    public override float RadicalVerticalGap(TFont font) =>
      ConstantFromTable(font, nameof(RadicalVerticalGap));
    public JsonMathTable(TestFontMeasurer fontMeasurer, JToken mathTable,
                         TestGlyphNameProvider glyphNameProvider,
                         IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider) {
      JObject GetTable(string name) =>
        _mathTable[name] as JObject ?? throw new System.ArgumentException($"Table not found: {name}", nameof(mathTable));
      FontMeasurer = fontMeasurer;
      GlyphNameProvider = glyphNameProvider;
      GlyphBoundsProvider = glyphBoundsProvider;
      _mathTable = mathTable;
      _constantsDictionary = GetTable("constants");
      _vAssemblyTable = GetTable("v_assembly");
      _hAssemblyTable = GetTable("h_assembly");
      _italicTable = GetTable("italic");
    }
    // different from _ConstantFromTable in that the _ConstantFromTable requires
    // a font and uses _FontUnitsToPt, while this is just a straight percentage.
    protected override short ScriptPercentScaleDown(TFont font) =>
      _constantsDictionary[nameof(ScriptPercentScaleDown)]!.Value<short>();
    protected override short ScriptScriptPercentScaleDown(TFont font) =>
      _constantsDictionary[nameof(ScriptScriptPercentScaleDown)]!.Value<short>();
    /*
     *     NSDictionary* italics = (NSDictionary*) _mathTable[kItalic];
    NSString* glyphName = [self.font getGlyphName:glyph];
    NSNumber* val = (NSNumber*) italics[glyphName];
    // if val is nil, this returns 0.
    return [self fontUnitsToPt:val.intValue];*/
    public override float GetItalicCorrection(TFont font, TGlyph glyph) =>
      _italicTable[GlyphNameProvider.GetGlyphName(glyph)] is JToken entry
      ? FontUnitsToPt(font, entry.Value<int>())
      : 0;
    public override float FractionDelimiterSize(TFont font) => font.PointSize * 1.01f;
    public override float FractionDelimiterDisplayStyleSize(TFont font) =>
      font.PointSize * 2.39f;
    public override float SuperscriptBaselineDropMax(TFont font) =>
      ConstantFromTable(font, nameof(SuperscriptBaselineDropMax));
    public override float SubscriptBaselineDropMin(TFont font) =>
      ConstantFromTable(font, nameof(SubscriptBaselineDropMin));
    public override float SubscriptShiftDown(TFont font) =>
      ConstantFromTable(font, nameof(SubscriptShiftDown));
    public override float SubscriptTopMax(TFont font) =>
      ConstantFromTable(font, nameof(SubscriptTopMax));
    public override float SuperscriptShiftUp(TFont font) =>
      ConstantFromTable(font, nameof(SuperscriptShiftUp));
    public override float SuperscriptShiftUpCramped(TFont font) =>
      ConstantFromTable(font, nameof(SuperscriptShiftUpCramped));
    public override float SuperscriptBottomMin(TFont font) =>
      ConstantFromTable(font, nameof(SuperscriptBottomMin));
    public override float SpaceAfterScript(TFont font) =>
      ConstantFromTable(font, nameof(SpaceAfterScript));
    public override float SubSuperscriptGapMin(TFont font) =>
      ConstantFromTable(font, nameof(SubSuperscriptGapMin));
    public override float SuperscriptBottomMaxWithSubscript(TFont font) =>
      ConstantFromTable(font, nameof(SuperscriptBottomMaxWithSubscript));
    #region fractions
    public override float FractionNumeratorDisplayStyleShiftUp(TFont font) =>
      ConstantFromTable(font, nameof(FractionNumeratorDisplayStyleShiftUp));
    public override float FractionNumeratorShiftUp(TFont font) =>
      ConstantFromTable(font, nameof(FractionNumeratorShiftUp));
    public override float StackTopDisplayStyleShiftUp(TFont font) =>
      ConstantFromTable(font, nameof(StackTopDisplayStyleShiftUp));
    public override float StackTopShiftUp(TFont font) =>
      ConstantFromTable(font, nameof(StackTopShiftUp));
    public override float FractionNumDisplayStyleGapMin(TFont font) =>
      ConstantFromTable(font, nameof(FractionNumDisplayStyleGapMin));
    public override float FractionNumeratorGapMin(TFont font) =>
      ConstantFromTable(font, nameof(FractionNumeratorGapMin));
    public override float FractionDenominatorDisplayStyleShiftDown(TFont font) =>
      ConstantFromTable(font, nameof(FractionDenominatorDisplayStyleShiftDown));
    public override float FractionDenominatorShiftDown(TFont font) =>
      ConstantFromTable(font, nameof(FractionDenominatorShiftDown));
    public override float StackBottomDisplayStyleShiftDown(TFont font) =>
      ConstantFromTable(font, nameof(StackBottomDisplayStyleShiftDown));
    public override float StackBottomShiftDown(TFont font) =>
      ConstantFromTable(font, nameof(StackBottomShiftDown));
    public override float FractionDenomDisplayStyleGapMin(TFont font) =>
      ConstantFromTable(font, nameof(FractionDenomDisplayStyleGapMin));
    public override float FractionDenominatorGapMin(TFont font) =>
      ConstantFromTable(font, nameof(FractionDenominatorGapMin));
    public override float AxisHeight(TFont font) =>
      ConstantFromTable(font, nameof(AxisHeight));
    public override float FractionRuleThickness(TFont font) =>
      ConstantFromTable(font, nameof(FractionRuleThickness));
    public override float StackDisplayStyleGapMin(TFont font) =>
      ConstantFromTable(font, nameof(StackDisplayStyleGapMin));
    public override float StackGapMin(TFont font) =>
      ConstantFromTable(font, nameof(StackGapMin));
    #endregion
    #region radicals
    public override float RadicalKernBeforeDegree(TFont font) =>
      ConstantFromTable(font, nameof(RadicalKernBeforeDegree));
    public override float RadicalKernAfterDegree(TFont font) =>
      ConstantFromTable(font, nameof(RadicalKernAfterDegree));
    protected override short RadicalDegreeBottomRaisePercent(TFont font) =>
      _constantsDictionary[nameof(RadicalDegreeBottomRaisePercent)]!.Value<short>();
    public override float RadicalRuleThickness(TFont font) =>
      ConstantFromTable(font, nameof(RadicalRuleThickness));
    public override float RadicalExtraAscender(TFont font) =>
      ConstantFromTable(font, nameof(RadicalExtraAscender));
    #endregion
    #region glyph assembly
    private const string _assemblyPartsKey = "parts";
    private const string _advanceKey = "advance";
    private const string _endConnectorKey = "endConnector";
    private const string _startConnectorKey = "startConnector";
    private const string _extenderKey = "extender";
    private const string _glyphKey = "glyph";
    public override IEnumerable<GlyphPart<TGlyph>>? GetVerticalGlyphAssembly(TGlyph rawGlyph, TFont font) =>
      _vAssemblyTable[GlyphNameProvider.GetGlyphName(rawGlyph)]?[_assemblyPartsKey] is JArray parts
      ? parts.Select(partInfo =>
        new GlyphPart<TGlyph>(
          GlyphNameProvider.GetGlyph(partInfo[_glyphKey]!.Value<string>()!),
          FontUnitsToPt(font, partInfo[_advanceKey]!.Value<int>()),
          FontUnitsToPt(font, partInfo[_startConnectorKey]!.Value<int>()),
          FontUnitsToPt(font, partInfo[_endConnectorKey]!.Value<int>()),
          partInfo[_extenderKey]!.Value<bool>()))
      // Should have been defined, but let's return null
      : null;
    public override IEnumerable<GlyphPart<TGlyph>>? GetHorizontalGlyphAssembly(TGlyph rawGlyph, TFont font) =>
      _hAssemblyTable[GlyphNameProvider.GetGlyphName(rawGlyph)]?[_assemblyPartsKey] is JArray parts
      ? parts.Select(partInfo =>
        new GlyphPart<TGlyph>(
          GlyphNameProvider.GetGlyph(partInfo[_glyphKey]!.Value<string>()!),
          FontUnitsToPt(font, partInfo[_advanceKey]!.Value<int>()),
          FontUnitsToPt(font, partInfo[_startConnectorKey]!.Value<int>()),
          FontUnitsToPt(font, partInfo[_endConnectorKey]!.Value<int>()),
          partInfo[_extenderKey]!.Value<bool>()))
      // Should have been defined, but let's return null
      : null;
    public override float MinConnectorOverlap(TFont font) => ConstantFromTable(font, nameof(MinConnectorOverlap));

    private const string VerticalVariantsKey = "v_variants";
    private const string HorizontalVariantsKey = "h_variants";
    public override (IEnumerable<TGlyph> variants, int count) GetVerticalVariantsForGlyph(TGlyph rawGlyph) =>
      GetVariantsForGlyph(rawGlyph, _mathTable[VerticalVariantsKey]!);
    public override (IEnumerable<TGlyph> variants, int count) GetHorizontalVariantsForGlyph(TGlyph rawGlyph) =>
      GetVariantsForGlyph(rawGlyph, _mathTable[HorizontalVariantsKey]!);
    private (IEnumerable<TGlyph> variants, int count) GetVariantsForGlyph(TGlyph rawGlyph, JToken variants) {
      var glyphName = GlyphNameProvider.GetGlyphName(rawGlyph);
      var variantGlyphs = variants[glyphName];
      if (variantGlyphs is not JArray variantGlyphsArray) {
        var outputGlyph = GlyphNameProvider.GetGlyph(glyphName);
        if (!outputGlyph.Equals(rawGlyph))
          throw new Atom.InvalidCodePathException
            ("GlyphNameProvider.GetGlyph(GlyphNameProvider.GetGlyphName(rawGlyph)) != rawGlyph");
        return (new TGlyph[] { outputGlyph }, 1);
      } else
        return
          (variantGlyphsArray.Select(variantObj =>
            GlyphNameProvider.GetGlyph(((JValue)variantObj).ToString()!)),
          variantGlyphsArray.Count);
    }
    public override TGlyph GetLargerGlyph(TFont font, TGlyph glyph) {
      var glyphName = GlyphNameProvider.GetGlyphName(glyph);
      if (_mathTable[VerticalVariantsKey]![glyphName] is JArray variantGlyphs) {
        foreach (var jVariant in variantGlyphs) {
          var variantName = jVariant.ToString();
          if (variantName != glyphName) {
            //return the first glyph with a different name.
            return GlyphNameProvider.GetGlyph(variantName);
          }
        }
      }
      return glyph;
    }
    #endregion
    public override float UpperLimitGapMin(TFont font) =>
      ConstantFromTable(font, nameof(UpperLimitGapMin));
    public override float UpperLimitBaselineRiseMin(TFont font) =>
      ConstantFromTable(font, nameof(UpperLimitBaselineRiseMin));
    public override float LowerLimitGapMin(TFont font) =>
      ConstantFromTable(font, nameof(LowerLimitGapMin));

    public override float LowerLimitBaselineDropMin(TFont font) =>
      ConstantFromTable(font, nameof(LowerLimitBaselineDropMin));
    #region overline/underline
    public override float UnderbarVerticalGap(TFont font) =>
      ConstantFromTable(font, nameof(UnderbarVerticalGap));
    public override float UnderbarRuleThickness(TFont font) =>
      ConstantFromTable(font, nameof(UnderbarRuleThickness));
    public override float OverbarVerticalGap(TFont font) =>
      ConstantFromTable(font, nameof(OverbarVerticalGap));
    public override float OverbarExtraAscender(TFont font) =>
      ConstantFromTable(font, nameof(OverbarExtraAscender));
    public override float OverbarRuleThickness(TFont font) =>
      ConstantFromTable(font, nameof(OverbarRuleThickness));
    #endregion
    public override float AccentBaseHeight(TFont font) =>
      ConstantFromTable(font, nameof(AccentBaseHeight));
    public override float GetTopAccentAdjustment(TFont font, TGlyph glyph) {
      var nameValue = _mathTable["accents"]![GlyphNameProvider.GetGlyphName(glyph)];
      if (nameValue != null) {
        return FontUnitsToPt(font, nameValue.Value<int>());
      } else {
        // If no top accent is defined then it is the center of the advance width.
        using var glyphs = new Atom.RentedArray<TGlyph>(glyph);
        var (_, Total) = GlyphBoundsProvider.GetAdvancesForGlyphs(font, glyphs.Result, 1);
        return Total / 2;
      }
    }
  }
}