using System;
using System.Collections.Generic;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;

namespace CSharpMath.Rendering {
  public class MathTable : FontMathTable<MathFont, Glyph> {
    protected readonly MathConstants _constants;
    protected readonly Func<ushort, Typography.OpenFont.Glyph> _lookup;

    public MathTable(Typeface typeface) => (_constants, _lookup) = (typeface.MathConsts ??
      throw new ArgumentException($"{nameof(typeface)}.{nameof(typeface.MathConsts)} is {null}."), typeface.GetGlyphByIndex);

    float ReadRecord(MathValueRecord rec, MathFont font) =>
      rec.Value * font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);

    protected override short ScriptPercentScaleDown => _constants.ScriptPercentScaleDown;

    protected override short ScriptScriptPercentScaleDown => _constants.ScriptScriptPercentScaleDown;

    public override float AxisHeight(MathFont font) => ReadRecord(_constants.AxisHeight, font);

    public override float FractionDenomDisplayStyleGapMin(MathFont font) => ReadRecord(_constants.FractionDenomDisplayStyleGapMin, font);

    public override float FractionDenominatorDisplayStyleShiftDown(MathFont font) => ReadRecord(_constants.FractionDenominatorDisplayStyleShiftDown, font);

    public override float FractionDenominatorGapMin(MathFont font) => ReadRecord(_constants.FractionDenominatorGapMin, font);

    public override float FractionDenominatorShiftDown(MathFont font) => ReadRecord(_constants.FractionDenominatorShiftDown, font);

    public override float FractionNumDisplayStyleGapMin(MathFont font) => ReadRecord(_constants.FractionNumDisplayStyleGapMin, font);

    public override float FractionNumeratorDisplayStyleShiftUp(MathFont font) => ReadRecord(_constants.FractionNumeratorDisplayStyleShiftUp, font);

    public override float FractionNumeratorGapMin(MathFont font) => ReadRecord(_constants.FractionNumeratorGapMin, font);

    public override float FractionNumeratorShiftUp(MathFont font) => ReadRecord(_constants.FractionNumeratorShiftUp, font);

    public override float FractionRuleThickness(MathFont font) => ReadRecord(_constants.FractionRuleThickness, font);

    internal static Dictionary<Glyph, Bounds> OriginalBounds = new Dictionary<Glyph, Bounds>();
    Glyph[] GetVariants(MathGlyphConstruction glyphs, Bounds bounds) {
      var records = glyphs.glyphVariantRecords;
      if (records == null) return Array.Empty<Glyph>();
      var variants = new Glyph[records.Length];
      for (int i = 0; i < records.Length; i++) {
        var variant = records[i].VariantGlyph;
        variants[i] = _lookup(variant);
        OriginalBounds[variants[i]] = bounds;
      }
      return variants;
    }

    public override Glyph[] GetHorizontalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.MathGlyphInfo?.HoriGlyphConstruction, rawGlyph.Bounds);

    public override float GetItalicCorrection(MathFont font, Glyph glyph) =>
      glyph.MathGlyphInfo.ItalicCorrection?.Value * font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize) ?? 0;

    public override Glyph GetLargerGlyph(MathFont font, Glyph glyph) {
      var variants = glyph.MathGlyphInfo.VertGlyphConstruction.glyphVariantRecords;
      var glyphIndex = glyph.GlyphIndex;
        foreach (var variant in variants) {
          var variantIndex = variant.VariantGlyph;
          if (variantIndex != glyphIndex) {
            //return the first glyph with a different index.
            var variantGlyph = _lookup(variantIndex);
            return variantGlyph;
          }
        }
      return glyph;
    }

    public override GlyphPart<Glyph>[] GetVerticalGlyphAssembly(Glyph rawGlyph, MathFont font) {
      var records = rawGlyph.MathGlyphInfo.VertGlyphConstruction.GlyphAsm_GlyphPartRecords;
      var scale = font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
      var parts = new GlyphPart<Glyph>[records.Length];
      for (int i = 0; i < records.Length; i++) parts[i] = new GlyphPart<Glyph> {
        EndConnectorLength = records[i].EndConnectorLength * scale,
        FullAdvance = records[i].FullAdvance * scale,
        Glyph = _lookup(records[i].GlyphId),
        IsExtender = records[i].IsExtender,
        StartConnectorLength = records[i].StartConnectorLength * scale
      };
      return parts;
    }

    public override Glyph[] GetVerticalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.MathGlyphInfo?.VertGlyphConstruction, rawGlyph.Bounds);

    public override float LowerLimitBaselineDropMin(MathFont font) => ReadRecord(_constants.LowerLimitBaselineDropMin, font);

    public override float LowerLimitGapMin(MathFont font) => ReadRecord(_constants.LowerLimitGapMin, font);

    public override float MinConnectorOverlap(MathFont font) =>
      _constants.MinConnectorOverlap * font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);

    protected override short RadicalDegreeBottomRaisePercent(MathFont font) => _constants.RadicalDegreeBottomRaisePercent;

    public override float RadicalDisplayStyleVerticalGap(MathFont font) => ReadRecord(_constants.RadicalDisplayStyleVerticalGap, font);

    public override float RadicalExtraAscender(MathFont font) => ReadRecord(_constants.RadicalExtraAscender, font);

    public override float RadicalKernAfterDegree(MathFont font) => ReadRecord(_constants.RadicalKernAfterDegree, font);

    public override float RadicalKernBeforeDegree(MathFont font) => ReadRecord(_constants.RadicalKernBeforeDegree, font);

    public override float RadicalRuleThickness(MathFont font) => ReadRecord(_constants.RadicalRuleThickness, font);

    public override float RadicalVerticalGap(MathFont font) => ReadRecord(_constants.RadicalVerticalGap, font);

    public override float SpaceAfterScript(MathFont font) => ReadRecord(_constants.SpaceAfterScript, font);

    public override float StackBottomDisplayStyleShiftDown(MathFont font) => ReadRecord(_constants.StackBottomDisplayStyleShiftDown, font);

    public override float StackBottomShiftDown(MathFont font) => ReadRecord(_constants.StackBottomShiftDown, font);

    public override float StackDisplayStyleGapMin(MathFont font) => ReadRecord(_constants.StackDisplayStyleGapMin, font);

    public override float StackGapMin(MathFont font) => ReadRecord(_constants.StackGapMin, font);

    public override float StackTopDisplayStyleShiftUp(MathFont font) => ReadRecord(_constants.StackTopDisplayStyleShiftUp, font);

    public override float StackTopShiftUp(MathFont font) => ReadRecord(_constants.StackTopShiftUp, font);

    public override float SubscriptBaselineDropMin(MathFont font) => ReadRecord(_constants.SubscriptBaselineDropMin, font);

    public override float SubscriptShiftDown(MathFont font) => ReadRecord(_constants.SubscriptShiftDown, font);

    public override float SubscriptTopMax(MathFont font) => ReadRecord(_constants.SubscriptTopMax, font);

    public override float SubSuperscriptGapMin(MathFont font) => ReadRecord(_constants.SubSuperscriptGapMin, font);

    public override float SuperscriptBaselineDropMax(MathFont font) => ReadRecord(_constants.SuperscriptBaselineDropMax, font);

    public override float SuperscriptBottomMaxWithSubscript(MathFont font) => ReadRecord(_constants.SuperscriptBottomMaxWithSubscript, font);

    public override float SuperscriptBottomMin(MathFont font) => ReadRecord(_constants.SuperscriptBottomMin, font);

    public override float SuperscriptShiftUp(MathFont font) => ReadRecord(_constants.SuperscriptShiftUp, font);

    public override float SuperscriptShiftUpCramped(MathFont font) => ReadRecord(_constants.SuperscriptShiftUpCramped, font);

    public override float UpperLimitBaselineRiseMin(MathFont font) => ReadRecord(_constants.UpperLimitBaselineRiseMin, font);

    public override float UpperLimitGapMin(MathFont font) => ReadRecord(_constants.UpperLimitGapMin, font);
  }

  internal static class GlyphExtensions {
    public static Bounds GetOriginalBounds(this Glyph rawGlyph) =>
      MathTable.OriginalBounds.TryGetValue(rawGlyph, out var value) ? value : rawGlyph.Bounds;
  }
}
