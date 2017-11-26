using CSharpMath.FrontEnd;
using Newtonsoft.Json.Linq;
using System;

namespace CSharpMath.Display.Text {
  internal class FontMathTable {
    private int _unitsPerEm;
    private float _fontSize;
    private JObject _mathTable;

    private WeakReference<MathFont> _fontReference { get; }
    private MathFont _font {
      get {
        MathFont font = null;
        _fontReference?.TryGetTarget(out font);
        return font;
      }
    }

    private JObject _constantsDictionary
      => _mathTable.Root["constants"] as JObject;

    private float _ConstantFromTable(string constantName) {
      var value = _constantsDictionary[constantName];
      throw new NotImplementedException();
    }

    private float _FontUnitsToPt(int fontUnits)
      => fontUnits * _fontSize / _unitsPerEm;

    public float ScriptScriptScaleDown { get; internal set; }
    public float ScriptScaleDown { get; internal set; }
    public float MuUnit => _font.PointSize / 18f;

    public float RadicalDisplayStyleVerticalGap { get; internal set; }
    public float RadicalVerticalGap { get; internal set; }

    public FontMathTable(MathFont font, JObject mathTable) {
      _unitsPerEm = FontMeasurers.Current.GetUnitsPerEm(font);
      _fontSize = _font.PointSize;
      _mathTable = mathTable;
    }
  }
}
