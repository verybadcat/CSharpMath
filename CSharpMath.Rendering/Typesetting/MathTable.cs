using System;
using System.Collections.Generic;
using CSharpMath.Display;
using CSharpMath.FrontEnd;
using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;

namespace CSharpMath.Rendering {
  public class MathTable : FontMathTable<MathFonts, Glyph> {
    float ReadRecord(MathValueRecord rec, MathFonts font) =>
      rec.Value * font.MathTypeface.CalculateScaleToPixelFromPointSize(font.PointSize);

    protected override short ScriptPercentScaleDown(MathFonts font) => font.MathConsts.ScriptPercentScaleDown;

    protected override short ScriptScriptPercentScaleDown(MathFonts font) => font.MathConsts.ScriptScriptPercentScaleDown;

    public override float AxisHeight(MathFonts font) => ReadRecord(font.MathConsts.AxisHeight, font);

    public override float FractionDenomDisplayStyleGapMin(MathFonts font) => ReadRecord(font.MathConsts.FractionDenomDisplayStyleGapMin, font);

    public override float FractionDenominatorDisplayStyleShiftDown(MathFonts font) => ReadRecord(font.MathConsts.FractionDenominatorDisplayStyleShiftDown, font);

    public override float FractionDenominatorGapMin(MathFonts font) => ReadRecord(font.MathConsts.FractionDenominatorGapMin, font);

    public override float FractionDenominatorShiftDown(MathFonts font) => ReadRecord(font.MathConsts.FractionDenominatorShiftDown, font);

    public override float FractionNumDisplayStyleGapMin(MathFonts font) => ReadRecord(font.MathConsts.FractionNumDisplayStyleGapMin, font);

    public override float FractionNumeratorDisplayStyleShiftUp(MathFonts font) => ReadRecord(font.MathConsts.FractionNumeratorDisplayStyleShiftUp, font);

    public override float FractionNumeratorGapMin(MathFonts font) => ReadRecord(font.MathConsts.FractionNumeratorGapMin, font);

    public override float FractionNumeratorShiftUp(MathFonts font) => ReadRecord(font.MathConsts.FractionNumeratorShiftUp, font);

    public override float FractionRuleThickness(MathFonts font) => ReadRecord(font.MathConsts.FractionRuleThickness, font);

    internal static Dictionary<Glyph, Bounds> OriginalBounds = new Dictionary<Glyph, Bounds>();
    Glyph[] GetVariants(Typeface typeface, MathGlyphConstruction glyphs, Bounds bounds) {
      var records = glyphs.glyphVariantRecords;
      if (records == null) return Array.Empty<Glyph>();
      var variants = new Glyph[records.Length];
      for (int i = 0; i < records.Length; i++) {
        var variant = records[i].VariantGlyph;
        variants[i] = new Glyph(typeface, typeface.GetGlyphByIndex(variant));
        OriginalBounds[variants[i]] = bounds;
      }
      return variants;
    }

    public override Glyph[] GetHorizontalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.HoriGlyphConstruction, rawGlyph.Info.Bounds);

    public override float GetItalicCorrection(MathFonts font, Glyph glyph) =>
      glyph.Info.MathGlyphInfo?.ItalicCorrection?.Value * glyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize) ?? 0;

    public override Glyph GetLargerGlyph(MathFonts font, Glyph glyph) {
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

    public override GlyphPart<Glyph>[] GetVerticalGlyphAssembly(Glyph rawGlyph, MathFonts font) {
      var records = rawGlyph.Info.MathGlyphInfo.VertGlyphConstruction.GlyphAsm_GlyphPartRecords;
      var scale = rawGlyph.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize);
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
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.VertGlyphConstruction, rawGlyph.Info.Bounds);

    public override float LowerLimitBaselineDropMin(MathFonts font) => ReadRecord(font.MathConsts.LowerLimitBaselineDropMin, font);

    public override float LowerLimitGapMin(MathFonts font) => ReadRecord(font.MathConsts.LowerLimitGapMin, font);

    public override float MinConnectorOverlap(MathFonts font) =>
      font.MathConsts.MinConnectorOverlap * font.MathTypeface.CalculateScaleToPixelFromPointSize(font.PointSize);

    protected override short RadicalDegreeBottomRaisePercent(MathFonts font) => font.MathConsts.RadicalDegreeBottomRaisePercent;

    public override float RadicalDisplayStyleVerticalGap(MathFonts font) => ReadRecord(font.MathConsts.RadicalDisplayStyleVerticalGap, font);

    public override float RadicalExtraAscender(MathFonts font) => ReadRecord(font.MathConsts.RadicalExtraAscender, font);

    public override float RadicalKernAfterDegree(MathFonts font) => ReadRecord(font.MathConsts.RadicalKernAfterDegree, font);

    public override float RadicalKernBeforeDegree(MathFonts font) => ReadRecord(font.MathConsts.RadicalKernBeforeDegree, font);

    public override float RadicalRuleThickness(MathFonts font) => ReadRecord(font.MathConsts.RadicalRuleThickness, font);

    public override float RadicalVerticalGap(MathFonts font) => ReadRecord(font.MathConsts.RadicalVerticalGap, font);

    public override float SpaceAfterScript(MathFonts font) => ReadRecord(font.MathConsts.SpaceAfterScript, font);

    public override float StackBottomDisplayStyleShiftDown(MathFonts font) => ReadRecord(font.MathConsts.StackBottomDisplayStyleShiftDown, font);

    public override float StackBottomShiftDown(MathFonts font) => ReadRecord(font.MathConsts.StackBottomShiftDown, font);

    public override float StackDisplayStyleGapMin(MathFonts font) => ReadRecord(font.MathConsts.StackDisplayStyleGapMin, font);

    public override float StackGapMin(MathFonts font) => ReadRecord(font.MathConsts.StackGapMin, font);

    public override float StackTopDisplayStyleShiftUp(MathFonts font) => ReadRecord(font.MathConsts.StackTopDisplayStyleShiftUp, font);

    public override float StackTopShiftUp(MathFonts font) => ReadRecord(font.MathConsts.StackTopShiftUp, font);

    public override float SubscriptBaselineDropMin(MathFonts font) => ReadRecord(font.MathConsts.SubscriptBaselineDropMin, font);

    public override float SubscriptShiftDown(MathFonts font) => ReadRecord(font.MathConsts.SubscriptShiftDown, font);

    public override float SubscriptTopMax(MathFonts font) => ReadRecord(font.MathConsts.SubscriptTopMax, font);

    public override float SubSuperscriptGapMin(MathFonts font) => ReadRecord(font.MathConsts.SubSuperscriptGapMin, font);

    public override float SuperscriptBaselineDropMax(MathFonts font) => ReadRecord(font.MathConsts.SuperscriptBaselineDropMax, font);

    public override float SuperscriptBottomMaxWithSubscript(MathFonts font) => ReadRecord(font.MathConsts.SuperscriptBottomMaxWithSubscript, font);

    public override float SuperscriptBottomMin(MathFonts font) => ReadRecord(font.MathConsts.SuperscriptBottomMin, font);

    public override float SuperscriptShiftUp(MathFonts font) => ReadRecord(font.MathConsts.SuperscriptShiftUp, font);

    public override float SuperscriptShiftUpCramped(MathFonts font) => ReadRecord(font.MathConsts.SuperscriptShiftUpCramped, font);

    public override float UpperLimitBaselineRiseMin(MathFonts font) => ReadRecord(font.MathConsts.UpperLimitBaselineRiseMin, font);

    public override float UpperLimitGapMin(MathFonts font) => ReadRecord(font.MathConsts.UpperLimitGapMin, font);

    public override float UnderbarVerticalGap(MathFonts font) {
      throw new NotImplementedException();
    }

    public override float AccentBaseHeight(MathFonts font) {
      throw new NotImplementedException();
    }

    public override float GetTopAccentAdjustment(MathFonts font, Glyph glyph) {
      throw new NotImplementedException();
    }

    public override float UnderbarRuleThickness(MathFonts font) {
      throw new NotImplementedException();
    }

    public override float OverbarVerticalGap(MathFonts font) {
      throw new NotImplementedException();
    }

    public override float OverbarRuleThickness(MathFonts font) {
      throw new NotImplementedException();
    }

    public override float OverbarExtraAscender(MathFonts font) {
      throw new NotImplementedException();
    }
  }

  internal static class GlyphExtensions {
    public static Bounds GetOriginalBounds(this Glyph rawGlyph) =>
      MathTable.OriginalBounds.TryGetValue(rawGlyph, out var value) ? value : rawGlyph.Info.Bounds;
  }
}
