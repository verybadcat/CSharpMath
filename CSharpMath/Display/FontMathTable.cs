using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CSharpMath.Display.Text {
  public class FontMathTable<TGlyph> {
    private JToken _mathTable;
    private IFontMeasurer<TGlyph> _fontMeasurer;
    private IGlyphNameProvider<TGlyph> _glyphNameProvider;
    private float _unitsPerEm(MathFont<TGlyph> font)
      => _fontMeasurer.GetUnitsPerEm(font);

    private JObject _constantsDictionary
      => _mathTable["constants"] as JObject;

    private JObject _assemblyTable
      => _mathTable["v_assembly"] as JObject;

    private float _ConstantFromTable(MathFont<TGlyph> font, string constantName) {
      var value = _constantsDictionary[constantName].Value<int>();
      return _FontUnitsToPt(font, value);
    }

    private float PercentFromTable(string name)
      // different from _ConstantFromTable in that the _ConstantFromTable requires
      // a font and uses _FontUnitsToPt, while this is just a straight percentage.
      => _constantsDictionary[name].Value<int>() / 100f;

    private float _FontUnitsToPt(MathFont<TGlyph> font, int fontUnits)
      => fontUnits * font.PointSize / _unitsPerEm(font);

    public float MuUnit(MathFont<TGlyph> font) => font.PointSize / 18f;

    public float RadicalDisplayStyleVerticalGap { get; internal set; }
    public float RadicalVerticalGap { get; internal set; }

    public FontMathTable(IFontMeasurer<TGlyph> fontMeasurer, JToken mathTable, IGlyphNameProvider<TGlyph> glyphNameProvider) {
      _fontMeasurer = fontMeasurer;
      _mathTable = mathTable;
      _glyphNameProvider = glyphNameProvider;
    }

    public float GetStyleSize(LineStyle style, MathFont<TGlyph> font) {
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

    internal float GetItalicCorrection(object glyph) {
      // TODO: write. See GetItalicCorrection in MTFontMathTable.m.
      return 0;
    }
    public float FractionDelimiterSize(MathFont<TGlyph> font)
    => font.PointSize * 1.01f;

    public float FractionDelimiterDisplayStyleSize(MathFont<TGlyph> font)
    => font.PointSize * 2.39f;

    public float SuperscriptBaselineDropMax(MathFont<TGlyph> font)
      => _ConstantFromTable(font, @"SuperscriptShiftUp");

    public float SuperscriptBaselineDropMin(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SuperscriptBaselineDropMax");

    public float SubscriptBaselineDropMin(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SubscriptBaselineDropMin");

    internal float SubscriptShiftDown(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SubscriptShiftDown");

    internal float SubscriptTopMax(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SubscriptTopMax");

    internal float SuperscriptShiftUp(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SuperscriptShiftUp");

    internal float SuperscriptShiftUpCramped(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SuperscriptShiftUpCramped");

    internal float SuperscriptBottomMin(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SuperscriptBottomMin");

    internal float SpaceAfterScript(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SpaceAfterScript");

    internal float SubSuperscriptGapMin(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SubSuperscriptGapMin");

    internal float SuperscriptBottomMaxWithSubscript(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "SuperscriptBottomMaxWithSubscript");

    #region fractions
    internal float FractionNumeratorDisplayStyleShiftUp(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "FractionNumeratorDisplayStyleShiftUp");

    internal float FractionNumeratorShiftUp(MathFont<TGlyph> font)
  => _ConstantFromTable(font, "FractionNumeratorShiftUp");

    internal float StackTopDisplayStyleShiftUp(MathFont<TGlyph> font)
  => _ConstantFromTable(font, "StackTopDisplayStyleShiftUp");

    internal float StackTopShiftUp(MathFont<TGlyph> font)
  => _ConstantFromTable(font, "StackTopShiftUp");

    internal float FractionNumeratorDisplayStyleGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionNumDisplayStyleGapMin");

    internal float FractionNumeratorGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionNumeratorGapMin");

    internal float FractionDenominatorDisplayStyleShiftDown(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionDenominatorDisplayStyleShiftDown");

    internal float FractionDenominatorShiftDown(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionDenominatorShiftDown");

    internal float StackBottomDisplayStyleShiftDown(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "StackBottomDisplayStyleShiftDown");

    internal float StackBottomShiftDown(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "StackBottomShiftDown");

    internal float FractionDenominatorDisplayStyleGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionDenomDisplayStyleGapMin");

    internal float FractionDenominatorGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionDenominatorGapMin");

    internal float AxisHeight(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "AxisHeight");

    internal float FractionRuleThickness(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "FractionRuleThickness");

    internal float StackDisplayStyleGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "StackDisplayStyleGapMin");

    internal float StackGapMin(MathFont<TGlyph> font)
=> _ConstantFromTable(font, "StackGapMin");
    #endregion

    #region radicals
    internal float RadicalKernBeforeDegree(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "RadicalKernBeforeDegree");

    internal float RadicalKernAfterDegree(MathFont<TGlyph> font)
  => _ConstantFromTable(font, "RadicalKernAfterDegree");

    internal float RadicalDegreeBottomRaisePercent(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "RadicalDegreeBottomRaisePercent");

    internal float RadicalRuleThickness(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "RadicalRuleThickness");

    internal float RadicalExtraAscender(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "RadicalExtraAscender");
    #endregion
    #region glyph assembly

    private const string _assemblyPartsKey = "parts";
    private const string _advanceKey = "advance";
    private const string _endConnectorKey = "endConnector";
    private const string _startConnectorKey = "startConnector";
    private const string _extenderKey = "extender";
    private const string _glyphKey = "glyph";
    public GlyphPart<TGlyph>[] GetVerticalGlyphAssembly(TGlyph rawGlyph) {
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
        r.Add(new GlyphPart<TGlyph> {
          FullAdvance = partInfo[_advanceKey].Value<int>(),
          EndConnectorLength = partInfo[_endConnectorKey].Value<int>(),
          StartConnectorLength = partInfo[_startConnectorKey].Value<int>(),
          IsExtender = partInfo[_extenderKey].Value<bool>(),
          Glyph = _glyphNameProvider.GetGlyph(glyphName)
        });
      }
      return r.ToArray();
    }
    public float MinConnecterGap(MathFont<TGlyph> font)
      => _ConstantFromTable(font, "MinConnecterGap");
    #endregion
  }

}

