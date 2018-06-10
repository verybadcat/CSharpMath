using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Structures;

namespace CSharpMath.Rendering {
  public interface ICanvas : Typography.OpenFont.IGlyphTranslator {
    Color DefaultColor { get; set; }
    Color? CurrentColor { get; set; }
    PaintStyle CurrentStyle { get; set; }

    void FillColor();
    void StrokeRect(float left, float top, float width, float height);
    void AddLine(float x1, float y1, float x2, float y2, float lineThickness);
    void Save();
    void Translate(float dx, float dy);
    void Scale(float sx, float sy);
    void Restore();
    void FillText(string text, float x, float y, float pointSize);
  }
}