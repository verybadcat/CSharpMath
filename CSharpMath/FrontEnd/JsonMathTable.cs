using CSharpMath.Display;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.FrontEnd {
  /// <summary>Holds lots of constants for spacing between various visible elements.
  /// If you are writing a new front end, you can likely re-use the code here,
  /// but you will probably need to create your own json file that holds the
  /// actual constants. </summary>
  public class JsonMathTable<TFont, TGlyph> : FontMathTable<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {
    /// <summary>Dictionary object containing a zillion constants,
    /// typically loaded from a .json file.</summary>
    private readonly JToken _mathTable;

    public IFontMeasurer<TFont, TGlyph> FontMeasurer { get; set; }
    public IGlyphNameProvider<TGlyph> GlyphNameProvider { get; set; }
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; set; }

    protected float _unitsPerEm(TFont font)
      => FontMeasurer.GetUnitsPerEm(font);

    protected float _FontUnitsToPt(TFont font, int fontUnits)
      => fontUnits * font.PointSize / _unitsPerEm(font);

    private JObject _constantsDictionary
      => _mathTable["constants"] as JObject;

    private JObject _assemblyTable
      => _mathTable["v_assembly"] as JObject;

    private JObject _italicTable
    => _mathTable["italic"] as JObject;

    private float _ConstantFromTable(TFont font, string constantName) {
      var value = _constantsDictionary[constantName].Value<int>();
      return _FontUnitsToPt(font, value);
    }

    private float PercentFromTable(string name)
      // different from _ConstantFromTable in that the _ConstantFromTable requires
      // a font and uses _FontUnitsToPt, while this is just a straight percentage.
      => _constantsDictionary[name].Value<int>() / 100f;

    public override float RadicalDisplayStyleVerticalGap(TFont font) => _ConstantFromTable(font, nameof(RadicalDisplayStyleVerticalGap));
    public override float RadicalVerticalGap(TFont font) => _ConstantFromTable(font, nameof(RadicalVerticalGap));

    public JsonMathTable(IFontMeasurer<TFont, TGlyph> fontMeasurer, JToken mathTable, 
                         IGlyphNameProvider<TGlyph> glyphNameProvider,
                         IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider) {
      FontMeasurer = fontMeasurer;
      _mathTable = mathTable;
      GlyphNameProvider = glyphNameProvider;
      GlyphBoundsProvider = glyphBoundsProvider;
    }

    // different from _ConstantFromTable in that the _ConstantFromTable requires
    // a font and uses _FontUnitsToPt, while this is just a straight percentage.
    protected override short ScriptPercentScaleDown(TFont font) => _constantsDictionary[nameof(ScriptPercentScaleDown)].Value<short>();
    protected override short ScriptScriptPercentScaleDown(TFont font) => _constantsDictionary[nameof(ScriptScriptPercentScaleDown)].Value<short>();

    /*
     *     NSDictionary* italics = (NSDictionary*) _mathTable[kItalic];
    NSString* glyphName = [self.font getGlyphName:glyph];
    NSNumber* val = (NSNumber*) italics[glyphName];
    // if val is nil, this returns 0.
    return [self fontUnitsToPt:val.intValue];*/
    public override float GetItalicCorrection(TFont font, TGlyph glyph) {
      var glyphName = GlyphNameProvider.GetGlyphName(glyph);
      var entry = _italicTable[glyphName];
      if (entry == null) {
        return 0;
      }
      var intEntry = entry.Value<int>();
      return _FontUnitsToPt(font, intEntry);
    }
    public override float FractionDelimiterSize(TFont font)
    => font.PointSize * 1.01f;

    public override float FractionDelimiterDisplayStyleSize(TFont font)
    => font.PointSize * 2.39f;

    public override float SuperscriptBaselineDropMax(TFont font)
      => _ConstantFromTable(font, nameof(SuperscriptBaselineDropMax));

    public override float SubscriptBaselineDropMin(TFont font)
      => _ConstantFromTable(font, nameof(SubscriptBaselineDropMin));

    public override float SubscriptShiftDown(TFont font)
      => _ConstantFromTable(font, nameof(SubscriptShiftDown));

    public override float SubscriptTopMax(TFont font)
      => _ConstantFromTable(font, nameof(SubscriptTopMax));

    public override float SuperscriptShiftUp(TFont font)
      => _ConstantFromTable(font, nameof(SuperscriptShiftUp));

    public override float SuperscriptShiftUpCramped(TFont font)
      => _ConstantFromTable(font, nameof(SuperscriptShiftUpCramped));

    public override float SuperscriptBottomMin(TFont font)
      => _ConstantFromTable(font, nameof(SuperscriptBottomMin));

    public override float SpaceAfterScript(TFont font)
      => _ConstantFromTable(font, nameof(SpaceAfterScript));

    public override float SubSuperscriptGapMin(TFont font)
      => _ConstantFromTable(font, nameof(SubSuperscriptGapMin));

    public override float SuperscriptBottomMaxWithSubscript(TFont font)
      => _ConstantFromTable(font, nameof(SuperscriptBottomMaxWithSubscript));

    #region fractions
    public override float FractionNumeratorDisplayStyleShiftUp(TFont font)
      => _ConstantFromTable(font, nameof(FractionNumeratorDisplayStyleShiftUp));

    public override float FractionNumeratorShiftUp(TFont font)
      => _ConstantFromTable(font, nameof(FractionNumeratorShiftUp));

    public override float StackTopDisplayStyleShiftUp(TFont font)
      => _ConstantFromTable(font, nameof(StackTopDisplayStyleShiftUp));

    public override float StackTopShiftUp(TFont font)
      => _ConstantFromTable(font, nameof(StackTopShiftUp));

    public override float FractionNumDisplayStyleGapMin(TFont font)
      => _ConstantFromTable(font, nameof(FractionNumDisplayStyleGapMin));

    public override float FractionNumeratorGapMin(TFont font)
      => _ConstantFromTable(font, nameof(FractionNumeratorGapMin));

    public override float FractionDenominatorDisplayStyleShiftDown(TFont font)
      => _ConstantFromTable(font, nameof(FractionDenominatorDisplayStyleShiftDown));

    public override float FractionDenominatorShiftDown(TFont font)
      => _ConstantFromTable(font, nameof(FractionDenominatorShiftDown));

    public override float StackBottomDisplayStyleShiftDown(TFont font)
        => _ConstantFromTable(font, nameof(StackBottomDisplayStyleShiftDown));

    public override float StackBottomShiftDown(TFont font)
        => _ConstantFromTable(font, nameof(StackBottomShiftDown));

    public override float FractionDenomDisplayStyleGapMin(TFont font)
        => _ConstantFromTable(font, nameof(FractionDenomDisplayStyleGapMin));

    public override float FractionDenominatorGapMin(TFont font)
        => _ConstantFromTable(font, nameof(FractionDenominatorGapMin));

    public override float AxisHeight(TFont font)
        => _ConstantFromTable(font, nameof(AxisHeight));

    public override float FractionRuleThickness(TFont font)
        => _ConstantFromTable(font, nameof(FractionRuleThickness));

    public override float StackDisplayStyleGapMin(TFont font)
        => _ConstantFromTable(font, nameof(StackDisplayStyleGapMin));

    public override float StackGapMin(TFont font)
        => _ConstantFromTable(font, nameof(StackGapMin));
    #endregion

    #region radicals
    public override float RadicalKernBeforeDegree(TFont font)
      => _ConstantFromTable(font, nameof(RadicalKernBeforeDegree));

    public override float RadicalKernAfterDegree(TFont font)
  => _ConstantFromTable(font, nameof(RadicalKernAfterDegree));

    protected override short RadicalDegreeBottomRaisePercent(TFont font)
      => _constantsDictionary[nameof(RadicalDegreeBottomRaisePercent)].Value<short>();

    public override float RadicalRuleThickness(TFont font)
      => _ConstantFromTable(font, nameof(RadicalRuleThickness));

    public override float RadicalExtraAscender(TFont font)
      => _ConstantFromTable(font, nameof(RadicalExtraAscender));
    #endregion
    #region glyph assembly

    private const string _assemblyPartsKey = "parts";
    private const string _advanceKey = "advance";
    private const string _endConnectorKey = "endConnector";
    private const string _startConnectorKey = "startConnector";
    private const string _extenderKey = "extender";
    private const string _glyphKey = "glyph";
    public override GlyphPart<TGlyph>[] GetVerticalGlyphAssembly(TGlyph rawGlyph, TFont font) {
      var glyphName = GlyphNameProvider.GetGlyphName(rawGlyph);
      var glyphAssemblyInfo = _assemblyTable[glyphName];
      if (glyphAssemblyInfo == null) {
        return null;
      }
      if (!(glyphAssemblyInfo[_assemblyPartsKey] is JArray parts)) {
        // Should have been defined, but let's return null
        return null;
      }
      List<GlyphPart<TGlyph>> r = new List<GlyphPart<TGlyph>>();
      foreach (JToken partInfo in parts) {
        var innerGlyphName = partInfo[_glyphKey];
        var endConnectorLength = _FontUnitsToPt(font, partInfo[_endConnectorKey].Value<int>());
        var startConnectorLength = _FontUnitsToPt(font, partInfo[_startConnectorKey].Value<int>());
        var fullAdvance = _FontUnitsToPt(font, partInfo[_advanceKey].Value<int>());
        var glyphPartName = partInfo[_glyphKey].Value<string>();
        r.Add(new GlyphPart<TGlyph> {
          EndConnectorLength = endConnectorLength,
          StartConnectorLength = startConnectorLength,
          FullAdvance = fullAdvance,
          IsExtender = partInfo[_extenderKey].Value<bool>(),
          Glyph = GlyphNameProvider.GetGlyph(glyphPartName)
        });
      }
      return r.ToArray();
    }
    public override float MinConnectorOverlap(TFont font)
    => _ConstantFromTable(font, nameof(MinConnectorOverlap));


    private const string VerticalVariantsKey = "v_variants";
    private const string HorizontalVariantsKey = "h_variants";
    public override TGlyph[] GetVerticalVariantsForGlyph(TGlyph rawGlyph) {
      var variants = _mathTable[VerticalVariantsKey];
      return GetVariantsForGlyph(rawGlyph, variants).ToArray();
    }

    public override TGlyph[] GetHorizontalVariantsForGlyph(TGlyph rawGlyph) {
      var variants = _mathTable[HorizontalVariantsKey];
      return GetVariantsForGlyph(rawGlyph, variants).ToArray();
    }

    private IEnumerable<TGlyph> GetVariantsForGlyph(TGlyph rawGlyph, JToken variants) {
      var glyphName = GlyphNameProvider.GetGlyphName(rawGlyph);
      var variantGlyphs = variants[glyphName];
      if (!(variantGlyphs is JArray variantGlyphsArray)) {
        var outputGlyph = GlyphNameProvider.GetGlyph(glyphName);
        // but are they ever different?
        if (!(outputGlyph.Equals(rawGlyph))) {
          throw new Exception("Just wanted to see if this ever happens");
        }
        yield return outputGlyph;
      } else {
        foreach (var variantObj in variantGlyphsArray) {
          var variantValue = variantObj as JValue;
          var variantName = variantValue.ToString();
          var aGlyph = GlyphNameProvider.GetGlyph(variantName);
          yield return aGlyph;
        }
      }
    }

    public override TGlyph GetLargerGlyph(TFont font, TGlyph glyph) {
      JToken variants = _mathTable[VerticalVariantsKey];
      var glyphName = GlyphNameProvider.GetGlyphName(glyph);
      if (variants[glyphName] is JArray variantGlyphs) {
        foreach (var jVariant in variantGlyphs) {
          var variantName = jVariant.ToString();
          if (variantName != glyphName) {
            //return the first glyph with a different name.
            var variantGlyph = GlyphNameProvider.GetGlyph(variantName);
            return variantGlyph;
          }
        }
      }
      return glyph;
    }


    #endregion

    public override float UpperLimitGapMin(TFont font)
        => _ConstantFromTable(font, nameof(UpperLimitGapMin));

    public override float UpperLimitBaselineRiseMin(TFont font)
        => _ConstantFromTable(font, nameof(UpperLimitBaselineRiseMin));

    public override float LowerLimitGapMin(TFont font)
        => _ConstantFromTable(font, nameof(LowerLimitGapMin));

    public override float LowerLimitBaselineDropMin(TFont font)
        => _ConstantFromTable(font, nameof(LowerLimitBaselineDropMin));

    #region overline/underline
    public override float UnderbarVerticalGap(TFont font)
        => _ConstantFromTable(font, nameof(UnderbarVerticalGap));

    public override float UnderbarRuleThickness(TFont font)
        => _ConstantFromTable(font, nameof(UnderbarRuleThickness));

    public override float OverbarVerticalGap(TFont font)
        => _ConstantFromTable(font, nameof(OverbarVerticalGap));

    public override float OverbarExtraAscender(TFont font) 
        => _ConstantFromTable(font, nameof(OverbarExtraAscender));

    public override float OverbarRuleThickness(TFont font)
        => _ConstantFromTable(font, nameof(OverbarRuleThickness));
    #endregion
    public override float AccentBaseHeight(TFont font)
        => _ConstantFromTable(font, nameof(AccentBaseHeight));

    public override float GetTopAccentAdjustment(TFont font, TGlyph glyph) {
      var accents = _mathTable["accents"];
      var glyphName = GlyphNameProvider.GetGlyphName(glyph);
      var nameValue = accents[glyphName];
      if (nameValue!=null) {
        var intValue = nameValue.Value<int>();
        return _FontUnitsToPt(font, intValue);
      } else {
        // If no top accent is defined then it is the center of the advance width.
        var glyphs = new TGlyph[] { glyph };
        var advances = GlyphBoundsProvider.GetAdvancesForGlyphs(font, glyphs);
        return advances.Advances[0] / 2;
      }
    }

  }
}

