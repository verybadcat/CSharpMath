using System;
using System.Collections.Generic;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;

namespace CSharpMath.SkiaSharp
{
  public class SkiaMathTable : FontMathTable<SkiaMathFont, Glyph> {
    protected readonly MathConstants _constants;
    protected readonly Func<ushort, Glyph> _lookup;

    public SkiaMathTable(Typeface typeface) => (_constants, _lookup) = (typeface.MathConsts ??
      throw new ArgumentException($"{nameof(typeface)}.{nameof(typeface.MathConsts)} is {null}."), typeface.GetGlyphByIndex);

    float ReadRecord(MathValueRecord rec, SkiaMathFont font) =>
      rec.Value * font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);

    protected override short ScriptPercentScaleDown => _constants.ScriptPercentScaleDown;

    protected override short ScriptScriptPercentScaleDown => _constants.ScriptScriptPercentScaleDown;

    public override float AxisHeight(SkiaMathFont font) => ReadRecord(_constants.AxisHeight, font);

    public override float FractionDenomDisplayStyleGapMin(SkiaMathFont font) => ReadRecord(_constants.FractionDenomDisplayStyleGapMin, font);

    public override float FractionDenominatorDisplayStyleShiftDown(SkiaMathFont font) => ReadRecord(_constants.FractionDenominatorDisplayStyleShiftDown, font);

    public override float FractionDenominatorGapMin(SkiaMathFont font) => ReadRecord(_constants.FractionDenominatorGapMin, font);

    public override float FractionDenominatorShiftDown(SkiaMathFont font) => ReadRecord(_constants.FractionDenominatorShiftDown, font);

    public override float FractionNumDisplayStyleGapMin(SkiaMathFont font) => ReadRecord(_constants.FractionNumDisplayStyleGapMin, font);

    public override float FractionNumeratorDisplayStyleShiftUp(SkiaMathFont font) => ReadRecord(_constants.FractionNumeratorDisplayStyleShiftUp, font);

    public override float FractionNumeratorGapMin(SkiaMathFont font) => ReadRecord(_constants.FractionNumeratorGapMin, font);

    public override float FractionNumeratorShiftUp(SkiaMathFont font) => ReadRecord(_constants.FractionNumeratorShiftUp, font);

    public override float FractionRuleThickness(SkiaMathFont font) => ReadRecord(_constants.FractionRuleThickness, font);

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

    public override float GetItalicCorrection(SkiaMathFont font, Glyph glyph) =>
      glyph.MathGlyphInfo.ItalicCorrection?.Value * font.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize) ?? 0;

    public override Glyph GetLargerGlyph(SkiaMathFont font, Glyph glyph) {
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

    public override GlyphPart<Glyph>[] GetVerticalGlyphAssembly(Glyph rawGlyph, SkiaMathFont font) {
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

    public override float LowerLimitBaselineDropMin(SkiaMathFont font) => ReadRecord(_constants.LowerLimitBaselineDropMin, font);

    public override float LowerLimitGapMin(SkiaMathFont font) => ReadRecord(_constants.LowerLimitGapMin, font);

    public override float MinConnectorOverlap(SkiaMathFont font) => _constants.MinConnectorOverlap;

    protected override short RadicalDegreeBottomRaisePercent(SkiaMathFont font) => _constants.RadicalDegreeBottomRaisePercent;

    public override float RadicalDisplayStyleVerticalGap(SkiaMathFont font) => ReadRecord(_constants.RadicalDisplayStyleVerticalGap, font);

    public override float RadicalExtraAscender(SkiaMathFont font) => ReadRecord(_constants.RadicalExtraAscender, font);

    public override float RadicalKernAfterDegree(SkiaMathFont font) => ReadRecord(_constants.RadicalKernAfterDegree, font);

    public override float RadicalKernBeforeDegree(SkiaMathFont font) => ReadRecord(_constants.RadicalKernBeforeDegree, font);

    public override float RadicalRuleThickness(SkiaMathFont font) => ReadRecord(_constants.RadicalRuleThickness, font);

    public override float RadicalVerticalGap(SkiaMathFont font) => ReadRecord(_constants.RadicalVerticalGap, font);

    public override float SpaceAfterScript(SkiaMathFont font) => ReadRecord(_constants.SpaceAfterScript, font);

    public override float StackBottomDisplayStyleShiftDown(SkiaMathFont font) => ReadRecord(_constants.StackBottomDisplayStyleShiftDown, font);

    public override float StackBottomShiftDown(SkiaMathFont font) => ReadRecord(_constants.StackBottomShiftDown, font);

    public override float StackDisplayStyleGapMin(SkiaMathFont font) => ReadRecord(_constants.StackDisplayStyleGapMin, font);

    public override float StackGapMin(SkiaMathFont font) => ReadRecord(_constants.StackGapMin, font);

    public override float StackTopDisplayStyleShiftUp(SkiaMathFont font) => ReadRecord(_constants.StackTopDisplayStyleShiftUp, font);

    public override float StackTopShiftUp(SkiaMathFont font) => ReadRecord(_constants.StackTopShiftUp, font);

    public override float SubscriptBaselineDropMin(SkiaMathFont font) => ReadRecord(_constants.SubscriptBaselineDropMin, font);

    public override float SubscriptShiftDown(SkiaMathFont font) => ReadRecord(_constants.SubscriptShiftDown, font);

    public override float SubscriptTopMax(SkiaMathFont font) => ReadRecord(_constants.SubscriptTopMax, font);

    public override float SubSuperscriptGapMin(SkiaMathFont font) => ReadRecord(_constants.SubSuperscriptGapMin, font);

    public override float SuperscriptBaselineDropMax(SkiaMathFont font) => ReadRecord(_constants.SuperscriptBaselineDropMax, font);

    public override float SuperscriptBottomMaxWithSubscript(SkiaMathFont font) => ReadRecord(_constants.SuperscriptBottomMaxWithSubscript, font);

    public override float SuperscriptBottomMin(SkiaMathFont font) => ReadRecord(_constants.SuperscriptBottomMin, font);

    public override float SuperscriptShiftUp(SkiaMathFont font) => ReadRecord(_constants.SuperscriptShiftUp, font);

    public override float SuperscriptShiftUpCramped(SkiaMathFont font) => ReadRecord(_constants.SuperscriptShiftUpCramped, font);

    public override float UpperLimitBaselineRiseMin(SkiaMathFont font) => ReadRecord(_constants.UpperLimitBaselineRiseMin, font);

    public override float UpperLimitGapMin(SkiaMathFont font) => ReadRecord(_constants.UpperLimitGapMin, font);
  }

  internal static class GlyphExtensions {
    public static Bounds GetOriginalBounds(this Glyph rawGlyph) =>
      SkiaMathTable.OriginalBounds.TryGetValue(rawGlyph, out var value) ? value : rawGlyph.Bounds;
  }
}
