using CSharpMath.FrontEnd;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Display.Text {
  internal class FontMathTable {
    private int _unitsPerEm;
    private float _fontSize;
    private Dictionary<object, object> _mathTable;

    private WeakReference<Font> _fontReference { get; }
    private Font _font {
      get {
        Font font = null;
        _fontReference?.TryGetTarget(out font);
        return font;
      }
    }
    public FontMathTable(Font font, Dictionary<object, object> mathTable) {
      _unitsPerEm = FontMeasurers.Current.GetUnitsPerEm(font);
      _fontSize = _font.PointSize;
      _mathTable = mathTable;
    }
  }
}
