using System;
using System.Collections.Generic;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;

namespace CSharpMath.Rendering {
  public class MathTable : FontMathTable<MathFonts, Glyph> {
    float ReadRecord(MathValueRecord rec, MathFonts fonts) =>
      rec.Value * fonts.MathTypeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);

    protected override short ScriptPercentScaleDown(MathFonts fonts) => fonts.MathConsts.ScriptPercentScaleDown;

    protected override short ScriptScriptPercentScaleDown(MathFonts fonts) => fonts.MathConsts.ScriptScriptPercentScaleDown;

    public override float AxisHeight(MathFonts fonts) => ReadRecord(fonts.MathConsts.AxisHeight, fonts);

    public override float FractionDenomDisplayStyleGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionDenomDisplayStyleGapMin, fonts);

    public override float FractionDenominatorDisplayStyleShiftDown(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorDisplayStyleShiftDown, fonts);

    public override float FractionDenominatorGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorGapMin, fonts);

    public override float FractionDenominatorShiftDown(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorShiftDown, fonts);

    public override float FractionNumDisplayStyleGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionNumDisplayStyleGapMin, fonts);

    public override float FractionNumeratorDisplayStyleShiftUp(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorDisplayStyleShiftUp, fonts);

    public override float FractionNumeratorGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorGapMin, fonts);

    public override float FractionNumeratorShiftUp(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorShiftUp, fonts);

    public override float FractionRuleThickness(MathFonts fonts) => ReadRecord(fonts.MathConsts.FractionRuleThickness, fonts);

    internal static Dictionary<Glyph, Bounds> OriginalBounds = new Dictionary<Glyph, Bounds>();
    Glyph[] GetVariants(Typeface typeface, MathGlyphConstruction glyphs, Bounds bounds) {
      if (glyphs == null) return null;
      var records = glyphs.glyphVariantRecords;
      if (records == null) return null;
      var variants = new Glyph[records.Length];
      for (int i = 0; i < records.Length; i++) {
        var variant = records[i].VariantGlyph;
        variants[i] = new Glyph(typeface, typeface.GetGlyphByIndex(variant));
        OriginalBounds[variants[i]] = bounds;
      }
      return variants;
    }

    public override Glyph[] GetHorizontalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.HoriGlyphConstruction, rawGlyph.Info.Bounds) ?? new[] { rawGlyph };

    public override float GetItalicCorrection(MathFonts fonts, Glyph glyph) =>
      glyph.Info.MathGlyphInfo?.ItalicCorrection?.Value * glyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize) ?? 0;

    public override Glyph GetLargerGlyph(MathFonts fonts, Glyph glyph) {
      var variants = glyph.Info.MathGlyphInfo.VertGlyphConstruction.glyphVariantRecords;
      var glyphIndex = glyph.Info.GlyphIndex;
        foreach (var variant in variants) {
          var variantIndex = variant.VariantGlyph;
          if (variantIndex != glyphIndex) {
            //return the first glyph with a different index.
            var variantGlyph = glyph.Typeface.GetGlyphByIndex(variantIndex);
            return new Glyph(glyph.Typeface, variantGlyph);
          }
        }
      return glyph;
    }

    public override GlyphPart<Glyph>[] GetVerticalGlyphAssembly(Glyph rawGlyph, MathFonts fonts) {
      var records = rawGlyph.Info.MathGlyphInfo.VertGlyphConstruction.GlyphAsm_GlyphPartRecords;
      var scale = rawGlyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
      var parts = new GlyphPart<Glyph>[records.Length];
      for (int i = 0; i < records.Length; i++) parts[i] = new GlyphPart<Glyph> {
        EndConnectorLength = records[i].EndConnectorLength * scale,
        FullAdvance = records[i].FullAdvance * scale,
        Glyph = new Glyph(rawGlyph.Typeface, rawGlyph.Typeface.GetGlyphByIndex(records[i].GlyphId)),
        IsExtender = records[i].IsExtender,
        StartConnectorLength = records[i].StartConnectorLength * scale
      };
      return parts;
    }

    public override Glyph[] GetVerticalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.VertGlyphConstruction, rawGlyph.Info.Bounds) ?? new[] { rawGlyph };

    public override float LowerLimitBaselineDropMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.LowerLimitBaselineDropMin, fonts);

    public override float LowerLimitGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.LowerLimitGapMin, fonts);

    public override float MinConnectorOverlap(MathFonts fonts) =>
      fonts.MathConsts.MinConnectorOverlap * fonts.MathTypeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);

    protected override short RadicalDegreeBottomRaisePercent(MathFonts fonts) => fonts.MathConsts.RadicalDegreeBottomRaisePercent;

    public override float RadicalDisplayStyleVerticalGap(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalDisplayStyleVerticalGap, fonts);

    public override float RadicalExtraAscender(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalExtraAscender, fonts);

    public override float RadicalKernAfterDegree(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalKernAfterDegree, fonts);

    public override float RadicalKernBeforeDegree(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalKernBeforeDegree, fonts);

    public override float RadicalRuleThickness(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalRuleThickness, fonts);

    public override float RadicalVerticalGap(MathFonts fonts) => ReadRecord(fonts.MathConsts.RadicalVerticalGap, fonts);

    public override float SpaceAfterScript(MathFonts fonts) => ReadRecord(fonts.MathConsts.SpaceAfterScript, fonts);

    public override float StackBottomDisplayStyleShiftDown(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackBottomDisplayStyleShiftDown, fonts);

    public override float StackBottomShiftDown(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackBottomShiftDown, fonts);

    public override float StackDisplayStyleGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackDisplayStyleGapMin, fonts);

    public override float StackGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackGapMin, fonts);

    public override float StackTopDisplayStyleShiftUp(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackTopDisplayStyleShiftUp, fonts);

    public override float StackTopShiftUp(MathFonts fonts) => ReadRecord(fonts.MathConsts.StackTopShiftUp, fonts);

    public override float SubscriptBaselineDropMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.SubscriptBaselineDropMin, fonts);

    public override float SubscriptShiftDown(MathFonts fonts) => ReadRecord(fonts.MathConsts.SubscriptShiftDown, fonts);

    public override float SubscriptTopMax(MathFonts fonts) => ReadRecord(fonts.MathConsts.SubscriptTopMax, fonts);

    public override float SubSuperscriptGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.SubSuperscriptGapMin, fonts);

    public override float SuperscriptBaselineDropMax(MathFonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBaselineDropMax, fonts);

    public override float SuperscriptBottomMaxWithSubscript(MathFonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBottomMaxWithSubscript, fonts);

    public override float SuperscriptBottomMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBottomMin, fonts);

    public override float SuperscriptShiftUp(MathFonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptShiftUp, fonts);

    public override float SuperscriptShiftUpCramped(MathFonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptShiftUpCramped, fonts);

    public override float UpperLimitBaselineRiseMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.UpperLimitBaselineRiseMin, fonts);

    public override float UpperLimitGapMin(MathFonts fonts) => ReadRecord(fonts.MathConsts.UpperLimitGapMin, fonts);

    public override float UnderbarVerticalGap(MathFonts fonts) => ReadRecord(fonts.MathConsts.UnderbarVerticalGap, fonts);

    public override float AccentBaseHeight(MathFonts fonts) => ReadRecord(fonts.MathConsts.AccentBaseHeight, fonts);

    public override float GetTopAccentAdjustment(MathFonts fonts, Glyph glyph) =>
      glyph.Info.MathGlyphInfo?.TopAccentAttachment?.Value
      * glyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize)
      // If no top accent is defined then it is the center of the advance width.
      ?? glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex) / 2;

    public override float UnderbarRuleThickness(MathFonts fonts) => ReadRecord(fonts.MathConsts.UnderbarRuleThickness, fonts);
    
    public override float OverbarVerticalGap(MathFonts fonts) => ReadRecord(fonts.MathConsts.OverbarVerticalGap, fonts);

    public override float OverbarRuleThickness(MathFonts fonts) => ReadRecord(fonts.MathConsts.OverbarRuleThickness, fonts);

    public override float OverbarExtraAscender(MathFonts fonts) => ReadRecord(fonts.MathConsts.OverbarExtraAscender, fonts);
  }

  internal static class GlyphExtensions {
    public static Bounds GetOriginalBounds(this Glyph rawGlyph) =>
      MathTable.OriginalBounds.TryGetValue(rawGlyph, out var value) ? value : rawGlyph.Info.Bounds;
  }
}
