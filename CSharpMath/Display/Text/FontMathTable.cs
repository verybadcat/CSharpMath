using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  internal class FontMathTable {
    private int _unitsPerEm;
    private float _fontSize;
    private Dictionary<object, object> _mathTable;

    private WeakReference<MathFont> _fontReference { get; }
    private MathFont _font {
      get {
        MathFont font = null;
        _fontReference?.TryGetTarget(out font);
        return font;
      }
    }

    public float ScriptScriptScaleDown { get; internal set; }
    public float ScriptScaleDown { get; internal set; }
    public float MuUnit => _font.PointSize / 18f;

    public FontMathTable(MathFont font, Dictionary<object, object> mathTable) {
      _unitsPerEm = FontMeasurers.Current.GetUnitsPerEm(font);
      _fontSize = _font.PointSize;
      _mathTable = mathTable;
    }
  }
}
