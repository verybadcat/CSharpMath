using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.Display.Text {
  public class FontMathTable<TFont, TGlyph>
    where TFont: MathFont<TGlyph>{
    private JToken _mathTable;
    private IFontMeasurer<TFont, TGlyph> _fontMeasurer;
    private IGlyphNameProvider<TGlyph> _glyphNameProvider;
    private float _unitsPerEm(TFont font)
      => _fontMeasurer.GetUnitsPerEm(font);

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

    private float _FontUnitsToPt(TFont font, int fontUnits)
      => fontUnits * font.PointSize / _unitsPerEm(font);

    public float MuUnit(TFont font) => font.PointSize / 18f;

    public float RadicalDisplayStyleVerticalGap { get; internal set; }
    public float RadicalVerticalGap { get; internal set; }

    public FontMathTable(IFontMeasurer<TFont, TGlyph> fontMeasurer, JToken mathTable, IGlyphNameProvider<TGlyph> glyphNameProvider) {
      _fontMeasurer = fontMeasurer;
      _mathTable = mathTable;
      _glyphNameProvider = glyphNameProvider;
    }

    public float GetStyleSize(LineStyle style, TFont font) {
      float originalSize = font.PointSize;
      switch (style) {
        case LineStyle.Display:
        case LineStyle.Text:
          return originalSize;
        case LineStyle.Script:
          return originalSize * ScriptScaleDown;
        case LineStyle.ScriptScript:
          return originalSize * ScriptScriptScaleDown;
        default:
          throw new NotImplementedException();
      }
    }


    public float ScriptScaleDown => PercentFromTable("ScriptPercentScaleDown");
    public float ScriptScriptScaleDown => PercentFromTable("ScriptScriptPercentScaleDown");

    /*
     *     NSDictionary* italics = (NSDictionary*) _mathTable[kItalic];
    NSString* glyphName = [self.font getGlyphName:glyph];
    NSNumber* val = (NSNumber*) italics[glyphName];
    // if val is nil, this returns 0.
    return [self fontUnitsToPt:val.intValue];*/
    internal float GetItalicCorrection(TFont font, TGlyph glyph) {
      var glyphName = _glyphNameProvider.GetGlyphName(glyph);
      var entry = _italicTable[glyphName];
      if (entry == null) {
        return 0;
      }
      var intEntry = entry.Value<int>();
      return _FontUnitsToPt(font, intEntry);
    }
    public float FractionDelimiterSize(TFont font)
    => font.PointSize * 1.01f;

    public float FractionDelimiterDisplayStyleSize(TFont font)
    => font.PointSize * 2.39f;

    public float SuperscriptBaselineDropMax(TFont font)
      => _ConstantFromTable(font, @"SuperscriptShiftUp");

    public float SuperscriptBaselineDropMin(TFont font)
      => _ConstantFromTable(font, "SuperscriptBaselineDropMax");

    public float SubscriptBaselineDropMin(TFont font)
      => _ConstantFromTable(font, "SubscriptBaselineDropMin");

    internal float SubscriptShiftDown(TFont font)
      => _ConstantFromTable(font, "SubscriptShiftDown");

    internal float SubscriptTopMax(TFont font)
      => _ConstantFromTable(font, "SubscriptTopMax");

    internal float SuperscriptShiftUp(TFont font)
      => _ConstantFromTable(font, "SuperscriptShiftUp");

    internal float SuperscriptShiftUpCramped(TFont font)
      => _ConstantFromTable(font, "SuperscriptShiftUpCramped");

    internal float SuperscriptBottomMin(TFont font)
      => _ConstantFromTable(font, "SuperscriptBottomMin");

    internal float SpaceAfterScript(TFont font)
      => _ConstantFromTable(font, "SpaceAfterScript");

    internal float SubSuperscriptGapMin(TFont font)
      => _ConstantFromTable(font, "SubSuperscriptGapMin");

    internal float SuperscriptBottomMaxWithSubscript(TFont font)
      => _ConstantFromTable(font, "SuperscriptBottomMaxWithSubscript");

    #region fractions
    internal float FractionNumeratorDisplayStyleShiftUp(TFont font)
      => _ConstantFromTable(font, "FractionNumeratorDisplayStyleShiftUp");

    internal float FractionNumeratorShiftUp(TFont font)
  => _ConstantFromTable(font, "FractionNumeratorShiftUp");

    internal float StackTopDisplayStyleShiftUp(TFont font)
  => _ConstantFromTable(font, "StackTopDisplayStyleShiftUp");

    internal float StackTopShiftUp(TFont font)
  => _ConstantFromTable(font, "StackTopShiftUp");

    internal float FractionNumeratorDisplayStyleGapMin(TFont font)
=> _ConstantFromTable(font, "FractionNumDisplayStyleGapMin");

    internal float FractionNumeratorGapMin(TFont font)
=> _ConstantFromTable(font, "FractionNumeratorGapMin");

    internal float FractionDenominatorDisplayStyleShiftDown(TFont font)
=> _ConstantFromTable(font, "FractionDenominatorDisplayStyleShiftDown");

    internal float FractionDenominatorShiftDown(TFont font)
=> _ConstantFromTable(font, "FractionDenominatorShiftDown");

    internal float StackBottomDisplayStyleShiftDown(TFont font)
=> _ConstantFromTable(font, "StackBottomDisplayStyleShiftDown");

    internal float StackBottomShiftDown(TFont font)
=> _ConstantFromTable(font, "StackBottomShiftDown");

    internal float FractionDenominatorDisplayStyleGapMin(TFont font)
=> _ConstantFromTable(font, "FractionDenomDisplayStyleGapMin");

    internal float FractionDenominatorGapMin(TFont font)
=> _ConstantFromTable(font, "FractionDenominatorGapMin");

    internal float AxisHeight(TFont font)
=> _ConstantFromTable(font, "AxisHeight");

    internal float FractionRuleThickness(TFont font)
=> _ConstantFromTable(font, "FractionRuleThickness");

    internal float StackDisplayStyleGapMin(TFont font)
=> _ConstantFromTable(font, "StackDisplayStyleGapMin");

    internal float StackGapMin(TFont font)
=> _ConstantFromTable(font, "StackGapMin");
    #endregion

    #region radicals
    internal float RadicalKernBeforeDegree(TFont font)
      => _ConstantFromTable(font, "RadicalKernBeforeDegree");

    internal float RadicalKernAfterDegree(TFont font)
  => _ConstantFromTable(font, "RadicalKernAfterDegree");

    internal float RadicalDegreeBottomRaisePercent(TFont font)
      => _ConstantFromTable(font, "RadicalDegreeBottomRaisePercent");

    internal float RadicalRuleThickness(TFont font)
      => _ConstantFromTable(font, "RadicalRuleThickness");

    internal float RadicalExtraAscender(TFont font)
      => _ConstantFromTable(font, "RadicalExtraAscender");
    #endregion
    #region glyph assembly

    private const string _assemblyPartsKey = "parts";
    private const string _advanceKey = "advance";
    private const string _endConnectorKey = "endConnector";
    private const string _startConnectorKey = "startConnector";
    private const string _extenderKey = "extender";
    private const string _glyphKey = "glyph";
    public GlyphPart<TGlyph>[] GetVerticalGlyphAssembly(TGlyph rawGlyph, TFont font) {
      var glyphName = _glyphNameProvider.GetGlyphName(rawGlyph);
      var glyphAssemblyInfo = _assemblyTable[glyphName];
      if (glyphAssemblyInfo == null) {
        return null;
      }
      var parts = glyphAssemblyInfo[_assemblyPartsKey] as JArray;
      if (parts == null) {
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
          Glyph = _glyphNameProvider.GetGlyph(glyphPartName)
        });
      }
      return r.ToArray();
    }
    public float MinConnectorOverlap(TFont font)
    => _ConstantFromTable(font, "MinConnectorOverlap");


    private const string VerticalVariantsKey = "v_variants";
    private const string HorizontalVariantsKey = "h_variants";
    internal TGlyph[] GetVerticalVariantsForGlyph(TGlyph rawGlyph) 
    {
      var variants = _mathTable[VerticalVariantsKey];
      return GetVariantsForGlyph(rawGlyph, variants).ToArray();
    }

    internal TGlyph[] GetHorizontalVariantsForGlyph(TGlyph rawGlyph)
    {
      var variants = _mathTable[HorizontalVariantsKey];
      return GetVariantsForGlyph(rawGlyph, variants).ToArray();
    }

    private IEnumerable<TGlyph> GetVariantsForGlyph(TGlyph rawGlyph, JToken variants) {
      var glyphName = _glyphNameProvider.GetGlyphName(rawGlyph);
      var variantGlyphs = variants[glyphName];
      var variantGlyphsArray = variantGlyphs as JArray;
      if (variantGlyphsArray == null) {
        var outputGlyph = _glyphNameProvider.GetGlyph(glyphName);
         // but are they ever different?
        if (!(outputGlyph.Equals(rawGlyph))) {
          throw new Exception("Just wanted to see if this ever happens");
        }
        yield return outputGlyph;
      } else {
        foreach (var variantObj in variantGlyphsArray) {
          var variantValue = variantObj as JValue;
          var variantName = variantValue.ToString();
          var aGlyph = _glyphNameProvider.GetGlyph(variantName);
          yield return aGlyph;
        }
      }
    }
    #endregion
  }

}

