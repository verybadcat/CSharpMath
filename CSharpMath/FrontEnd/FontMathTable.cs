using CSharpMath.Display;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpMath.FrontEnd {
  /// <summary>Holds lots of constants for spacing between various visible elements.</summary>
  public abstract class FontMathTable<TFont, TGlyph>
    where TFont : MathFont<TGlyph> {

    public float MuUnit(TFont font) => font.PointSize / 18f;

    public abstract float RadicalDisplayStyleVerticalGap(TFont font);
    public abstract float RadicalVerticalGap(TFont font);

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


    public float ScriptScaleDown => ScriptPercentScaleDown / 100f;
    public float ScriptScriptScaleDown => ScriptScriptPercentScaleDown / 100f;
    protected abstract short ScriptPercentScaleDown { get; }
    protected abstract short ScriptScriptPercentScaleDown { get; }

    /*
     *     NSDictionary* italics = (NSDictionary*) _mathTable[kItalic];
    NSString* glyphName = [self.font getGlyphName:glyph];
    NSNumber* val = (NSNumber*) italics[glyphName];
    // if val is nil, this returns 0.
    return [self fontUnitsToPt:val.intValue];*/
    public abstract float GetItalicCorrection(TFont font, TGlyph glyph);
    public virtual float FractionDelimiterSize(TFont font)
    => font.PointSize * 1.01f;

    public virtual float FractionDelimiterDisplayStyleSize(TFont font)
    => font.PointSize * 2.39f;

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
    public abstract GlyphPart<TGlyph>[] GetVerticalGlyphAssembly(TGlyph rawGlyph, TFont font);
    public abstract float MinConnectorOverlap(TFont font);
    
    public abstract TGlyph[] GetVerticalVariantsForGlyph(TGlyph rawGlyph);
    public abstract TGlyph[] GetHorizontalVariantsForGlyph(TGlyph rawGlyph);

    public abstract TGlyph GetLargerGlyph(TFont font, TGlyph glyph);


    #endregion

    public abstract float UpperLimitGapMin(TFont font);

    public abstract float UpperLimitBaselineRiseMin(TFont font);

    public abstract float LowerLimitGapMin(TFont font);

    public abstract float LowerLimitBaselineDropMin(TFont font);
  }
}

