using CSharpMath.Atom;
using System;
using System.Collections.Generic;

namespace CSharpMath.Display.FrontEnd {
  /// <summary>Holds lots of constants for spacing between various visible elements.</summary>
  public abstract class FontMathTable<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public float MuUnit(TFont font) => font.PointSize / 18f;
    public abstract float RadicalDisplayStyleVerticalGap(TFont font);
    public abstract float RadicalVerticalGap(TFont font);
    public float GetStyleSize(LineStyle style, TFont font) => style switch {
      LineStyle.Display => font.PointSize,
      LineStyle.Text => font.PointSize,
      LineStyle.Script => font.PointSize * ScriptScaleDown(font),
      LineStyle.ScriptScript => font.PointSize * ScriptScriptScaleDown(font),
      _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Style is out of range.")
    };

    public float ScriptScaleDown(TFont font) => ScriptPercentScaleDown(font) / 100f;
    public float ScriptScriptScaleDown(TFont font) => ScriptScriptPercentScaleDown(font) / 100f;
    protected abstract short ScriptPercentScaleDown(TFont font);
    protected abstract short ScriptScriptPercentScaleDown(TFont font);
    /*
     *     NSDictionary* italics = (NSDictionary*) _mathTable[kItalic];
    NSString* glyphName = [self.font getGlyphName:glyph];
    NSNumber* val = (NSNumber*) italics[glyphName];
    // if val is nil, this returns 0.
    return [self fontUnitsToPt:val.intValue];*/
    public abstract float GetItalicCorrection(TFont font, TGlyph glyph);
    public virtual float FractionDelimiterSize(TFont font) => font.PointSize * 1.01f;
    public virtual float FractionDelimiterDisplayStyleSize(TFont font) =>
      font.PointSize * 2.39f;
    public abstract float SuperscriptBaselineDropMax(TFont font);
    public abstract float SubscriptBaselineDropMin(TFont font);
    public abstract float SubscriptShiftDown(TFont font);
    public abstract float SubscriptTopMax(TFont font);
    public abstract float SuperscriptShiftUp(TFont font);
    public abstract float SuperscriptShiftUpCramped(TFont font);
    public abstract float SuperscriptBottomMin(TFont font);
    public abstract float SpaceAfterScript(TFont font);
    public abstract float SubSuperscriptGapMin(TFont font);
    public abstract float SuperscriptBottomMaxWithSubscript(TFont font);
    #region fractions
    public abstract float FractionNumeratorDisplayStyleShiftUp(TFont font);
    public abstract float FractionNumeratorShiftUp(TFont font);
    public abstract float StackTopDisplayStyleShiftUp(TFont font);
    public abstract float StackTopShiftUp(TFont font);
    public abstract float FractionNumDisplayStyleGapMin(TFont font);
    public abstract float FractionNumeratorGapMin(TFont font);
    public abstract float FractionDenominatorDisplayStyleShiftDown(TFont font);
    public abstract float FractionDenominatorShiftDown(TFont font);
    public abstract float StackBottomDisplayStyleShiftDown(TFont font);
    public abstract float StackBottomShiftDown(TFont font);
    public abstract float FractionDenomDisplayStyleGapMin(TFont font);
    public abstract float FractionDenominatorGapMin(TFont font);
    public abstract float AxisHeight(TFont font);
    public abstract float FractionRuleThickness(TFont font);
    public abstract float StackDisplayStyleGapMin(TFont font);
    public abstract float StackGapMin(TFont font);
    #endregion
    #region radicals
    public abstract float RadicalKernBeforeDegree(TFont font);
    public abstract float RadicalKernAfterDegree(TFont font);
    public float RadicalDegreeBottomRaise(TFont font) => RadicalDegreeBottomRaisePercent(font) / 100f;
    protected abstract short RadicalDegreeBottomRaisePercent(TFont font);
    public abstract float RadicalRuleThickness(TFont font);
    public abstract float RadicalExtraAscender(TFont font);
    #endregion
    #region glyph assembly
    public abstract IEnumerable<GlyphPart<TGlyph>>? GetVerticalGlyphAssembly(TGlyph rawGlyph, TFont font);
    public abstract float MinConnectorOverlap(TFont font);
    public abstract (IEnumerable<TGlyph> variants, int count) GetVerticalVariantsForGlyph(TGlyph rawGlyph);
    public abstract (IEnumerable<TGlyph> variants, int count) GetHorizontalVariantsForGlyph(TGlyph rawGlyph);
    public abstract TGlyph GetLargerGlyph(TFont font, TGlyph glyph);
    #endregion
    public abstract float UpperLimitGapMin(TFont font);
    public abstract float UpperLimitBaselineRiseMin(TFont font);
    public abstract float LowerLimitGapMin(TFont font);
    public abstract float LowerLimitBaselineDropMin(TFont font);
    public abstract float UnderbarVerticalGap(TFont font);
    public abstract float UnderbarRuleThickness(TFont font);
    public abstract float OverbarVerticalGap(TFont font);
    public abstract float OverbarRuleThickness(TFont font);
    public abstract float OverbarExtraAscender(TFont font);
    public abstract float AccentBaseHeight(TFont font);
    public abstract float GetTopAccentAdjustment(TFont font, TGlyph glyph);
  }
}
