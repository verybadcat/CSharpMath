using CSharpMath.FrontEnd;
using Newtonsoft.Json.Linq;
using System;

namespace CSharpMath.Display.Text {
  public class FontMathTable {
    private JToken _mathTable;
    private float _unitsPerEm;

    private JObject _constantsDictionary
      => _mathTable["constants"] as JObject;

    private float _ConstantFromTable(string constantName) {
      var value = _constantsDictionary[constantName];
      throw new NotImplementedException();
    }

    private float _FontUnitsToPt(MathFont font, int fontUnits)
      => fontUnits * font.PointSize / _unitsPerEm;

    public float ScriptScriptScaleDown { get; internal set; }
    public float ScriptScaleDown { get; internal set; }
    public float MuUnit(MathFont font) => font.PointSize / 18f;

    public float RadicalDisplayStyleVerticalGap { get; internal set; }
    public float RadicalVerticalGap { get; internal set; }

    public FontMathTable(float unitsPerEm, JToken mathTable) {
      _unitsPerEm = unitsPerEm;
      _mathTable = mathTable;
    }
  }
}
