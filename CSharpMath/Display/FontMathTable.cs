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

    private float _ConstantFromTable(string constantName) {
      var value = _constantsDictionary[constantName];
      throw new NotImplementedException();
    }

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
    private float PercentFromTable(string name)
      => _ConstantFromTable(name) / 100;

    public float ScriptScaleDown => PercentFromTable("ScriptPercentScaleDown");
    public float ScriptScriptScaleDown => PercentFromTable("ScriptScriptPercentScaleDown");
  }
}
