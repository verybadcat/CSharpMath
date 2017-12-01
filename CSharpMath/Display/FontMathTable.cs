using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using Newtonsoft.Json.Linq;
using System;

namespace CSharpMath.Display.Text {
  public class FontMathTable {
    private JToken _mathTable;
    private IFontMeasurer _fontMeasurer;
    private float _unitsPerEm(MathFont font)
      => _fontMeasurer.GetUnitsPerEm(font);

    private JObject _constantsDictionary
      => _mathTable["constants"] as JObject;

    private float _ConstantFromTable(MathFont font, string constantName) {
      var value = _constantsDictionary[constantName].Value<int>();
      return _FontUnitsToPt(font, value);
    }

    private float PercentFromTable(string name)
      // different from _ConstantFromTable in that the _ConstantFromTable requires
      // a font and uses _FontUnitsToPt, while this is just a straight percentage.
      => _constantsDictionary[name].Value<int>() / 100f;

    private float _FontUnitsToPt(MathFont font, int fontUnits)
      => fontUnits * font.PointSize / _unitsPerEm(font);

    public float MuUnit(MathFont font) => font.PointSize / 18f;

    public float RadicalDisplayStyleVerticalGap { get; internal set; }
    public float RadicalVerticalGap { get; internal set; }

    public FontMathTable(IFontMeasurer fontMeasurer, JToken mathTable) {
      _fontMeasurer = fontMeasurer;
      _mathTable = mathTable;
    }

    public float GetStyleSize(LineStyle style, MathFont font) {
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

    internal float GetItalicCorrection(char v) {
      // TODO: write. See GetItalicCorrection in MTFontMathTable.m.
      return 0;
    }
    public float FractionDelimiterSize(MathFont font)
    => font.PointSize * 1.01f;

    public float SuperscriptBaselineDropMax(MathFont font)
      => _ConstantFromTable(font, @"SuperscriptShiftUp");

    public float SuperscriptBaselineDropMin(MathFont font)
      => _ConstantFromTable(font, "SuperscriptBaselineDropMax");

    public float SubscriptBaselineDropMin(MathFont font)
      => _ConstantFromTable(font, "SubscriptBaselineDropMin");
    
    internal float SubscriptShiftDown(MathFont font)
      => _ConstantFromTable(font, "SubscriptShiftDown");

    internal float SubscriptTopMax(MathFont font)
      => _ConstantFromTable(font, "SubscriptTopMax");

    internal float SuperscriptShiftUp(MathFont font)
      => _ConstantFromTable(font, "SuperscriptShiftUp");

    internal float SuperscriptShiftUpCramped(MathFont font)
      => _ConstantFromTable(font, "SuperscriptShiftUpCramped");

    internal float SuperscriptBottomMin(MathFont font)
      => _ConstantFromTable(font, "SuperscriptBottomMin");

    internal float SpaceAfterScript(MathFont font)
      => _ConstantFromTable(font, "SpaceAfterScript");

    internal float SubSuperscriptGapMin(MathFont font)
      => _ConstantFromTable(font, "SubSuperscriptGapMin");

    internal float SuperscriptBottomMaxWithSubscript(MathFont font)
      => _ConstantFromTable(font, "SuperscriptBottomMaxWithSubscript");

  }
}
