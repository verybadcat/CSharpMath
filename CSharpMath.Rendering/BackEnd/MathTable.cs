using System.Collections.Generic;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Display.FrontEnd;
using Typography.OpenFont;
using Typography.OpenFont.MathGlyphs;

namespace CSharpMath.Rendering.BackEnd {
  public class MathTable : FontMathTable<Fonts, Glyph> {
    private MathTable() { }
    public static MathTable Instance { get; } = new MathTable();
    float ReadRecord(MathValueRecord rec, Fonts fonts) =>
      rec.Value * fonts.MathTypeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
    protected override short ScriptPercentScaleDown(Fonts fonts) => fonts.MathConsts.ScriptPercentScaleDown;
    protected override short ScriptScriptPercentScaleDown(Fonts fonts) => fonts.MathConsts.ScriptScriptPercentScaleDown;
    public override float AxisHeight(Fonts fonts) => ReadRecord(fonts.MathConsts.AxisHeight, fonts);
    public override float FractionDenomDisplayStyleGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionDenomDisplayStyleGapMin, fonts);
    public override float FractionDenominatorDisplayStyleShiftDown(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorDisplayStyleShiftDown, fonts);
    public override float FractionDenominatorGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorGapMin, fonts);
    public override float FractionDenominatorShiftDown(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionDenominatorShiftDown, fonts);
    public override float FractionNumDisplayStyleGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionNumDisplayStyleGapMin, fonts);
    public override float FractionNumeratorDisplayStyleShiftUp(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorDisplayStyleShiftUp, fonts);
    public override float FractionNumeratorGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorGapMin, fonts);
    public override float FractionNumeratorShiftUp(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionNumeratorShiftUp, fonts);
    public override float FractionRuleThickness(Fonts fonts) => ReadRecord(fonts.MathConsts.FractionRuleThickness, fonts);
    (IEnumerable<Glyph> variants, int count)? GetVariants(Typeface typeface, MathGlyphConstruction? glyphs) {
      var records = glyphs?.GlyphVariantRecords;
      if (records == null) return null;
      return (records.Select(record => new Glyph(typeface, typeface.GetGlyph(record.VariantGlyph))), records.Length);
    }
    public override (IEnumerable<Glyph> variants, int count) GetHorizontalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.HoriGlyphConstruction) ?? (new[] { rawGlyph }, 1);
    public override (IEnumerable<Glyph> variants, int count) GetVerticalVariantsForGlyph(Glyph rawGlyph) =>
      GetVariants(rawGlyph.Typeface, rawGlyph.Info.MathGlyphInfo?.VertGlyphConstruction) ?? (new[] { rawGlyph }, 1);
    public override float GetItalicCorrection(Fonts fonts, Glyph glyph) =>
      glyph.Info.MathGlyphInfo?.ItalicCorrection?.Value * glyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize) ?? 0;
    public override Glyph GetLargerGlyph(Fonts fonts, Glyph glyph) {
      var variants = glyph.Info.MathGlyphInfo?.VertGlyphConstruction?.GlyphVariantRecords;
      var glyphIndex = glyph.Info.GlyphIndex;
      foreach (var variant in variants ?? System.Array.Empty<MathGlyphVariantRecord>()) {
        var variantIndex = variant.VariantGlyph;
        if (variantIndex != glyphIndex) {
          //return the first glyph with a different index.
          var variantGlyph = glyph.Typeface.GetGlyph(variantIndex);
          return new Glyph(glyph.Typeface, variantGlyph);
        }
      }
      return glyph;
    }
    public override IEnumerable<GlyphPart<Glyph>>? GetVerticalGlyphAssembly(Glyph rawGlyph, Fonts fonts) {
      var scale = rawGlyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
      return
        rawGlyph.Info.MathGlyphInfo?.VertGlyphConstruction?.GlyphAsm?.GlyphPartRecords.Select(record =>
        new GlyphPart<Glyph>(
          new Glyph(rawGlyph.Typeface, rawGlyph.Typeface.GetGlyph(record.GlyphId)),
          record.FullAdvance * scale,
          record.StartConnectorLength * scale,
          record.EndConnectorLength * scale,
          record.IsExtender));
    }
    public override float LowerLimitBaselineDropMin(Fonts fonts) => ReadRecord(fonts.MathConsts.LowerLimitBaselineDropMin, fonts);
    public override float LowerLimitGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.LowerLimitGapMin, fonts);
    public override float MinConnectorOverlap(Fonts fonts) =>
      fonts.MathConsts.MinConnectorOverlap * fonts.MathTypeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
    protected override short RadicalDegreeBottomRaisePercent(Fonts fonts) => fonts.MathConsts.RadicalDegreeBottomRaisePercent;
    public override float RadicalDisplayStyleVerticalGap(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalDisplayStyleVerticalGap, fonts);
    public override float RadicalExtraAscender(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalExtraAscender, fonts);
    public override float RadicalKernAfterDegree(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalKernAfterDegree, fonts);
    public override float RadicalKernBeforeDegree(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalKernBeforeDegree, fonts);
    public override float RadicalRuleThickness(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalRuleThickness, fonts);
    public override float RadicalVerticalGap(Fonts fonts) => ReadRecord(fonts.MathConsts.RadicalVerticalGap, fonts);
    public override float SpaceAfterScript(Fonts fonts) => ReadRecord(fonts.MathConsts.SpaceAfterScript, fonts);
    public override float StackBottomDisplayStyleShiftDown(Fonts fonts) => ReadRecord(fonts.MathConsts.StackBottomDisplayStyleShiftDown, fonts);
    public override float StackBottomShiftDown(Fonts fonts) => ReadRecord(fonts.MathConsts.StackBottomShiftDown, fonts);
    public override float StackDisplayStyleGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.StackDisplayStyleGapMin, fonts);
    public override float StackGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.StackGapMin, fonts);
    public override float StackTopDisplayStyleShiftUp(Fonts fonts) => ReadRecord(fonts.MathConsts.StackTopDisplayStyleShiftUp, fonts);
    public override float StackTopShiftUp(Fonts fonts) => ReadRecord(fonts.MathConsts.StackTopShiftUp, fonts);
    public override float SubscriptBaselineDropMin(Fonts fonts) => ReadRecord(fonts.MathConsts.SubscriptBaselineDropMin, fonts);
    public override float SubscriptShiftDown(Fonts fonts) => ReadRecord(fonts.MathConsts.SubscriptShiftDown, fonts);
    public override float SubscriptTopMax(Fonts fonts) => ReadRecord(fonts.MathConsts.SubscriptTopMax, fonts);
    public override float SubSuperscriptGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.SubSuperscriptGapMin, fonts);
    public override float SuperscriptBaselineDropMax(Fonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBaselineDropMax, fonts);
    public override float SuperscriptBottomMaxWithSubscript(Fonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBottomMaxWithSubscript, fonts);
    public override float SuperscriptBottomMin(Fonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptBottomMin, fonts);
    public override float SuperscriptShiftUp(Fonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptShiftUp, fonts);
    public override float SuperscriptShiftUpCramped(Fonts fonts) => ReadRecord(fonts.MathConsts.SuperscriptShiftUpCramped, fonts);
    public override float UpperLimitBaselineRiseMin(Fonts fonts) => ReadRecord(fonts.MathConsts.UpperLimitBaselineRiseMin, fonts);
    public override float UpperLimitGapMin(Fonts fonts) => ReadRecord(fonts.MathConsts.UpperLimitGapMin, fonts);
    public override float UnderbarVerticalGap(Fonts fonts) => ReadRecord(fonts.MathConsts.UnderbarVerticalGap, fonts);
    public override float AccentBaseHeight(Fonts fonts) => ReadRecord(fonts.MathConsts.AccentBaseHeight, fonts);
    public override float GetTopAccentAdjustment(Fonts fonts, Glyph glyph) =>
      (glyph.Info.MathGlyphInfo?.TopAccentAttachment?.Value
      // If no top accent is defined then it is the center of the advance width.
      ?? glyph.Typeface.GetHAdvanceWidthFromGlyphIndex(glyph.Info.GlyphIndex) / 2)
      //remember to scale the unit
      * glyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
    public override float UnderbarRuleThickness(Fonts fonts) => ReadRecord(fonts.MathConsts.UnderbarRuleThickness, fonts);
    public override float OverbarVerticalGap(Fonts fonts) => ReadRecord(fonts.MathConsts.OverbarVerticalGap, fonts);
    public override float OverbarRuleThickness(Fonts fonts) => ReadRecord(fonts.MathConsts.OverbarRuleThickness, fonts);
    public override float OverbarExtraAscender(Fonts fonts) => ReadRecord(fonts.MathConsts.OverbarExtraAscender, fonts);
  }
}
