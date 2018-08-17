using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  using Display = IDisplay<Fonts, Glyph>;
  public class TextDisplayLineBuilder {
    readonly Queue<Display> _queue = new Queue<Display>();

    public float Ascent { get; private set; }
    public float Descent { get; private set; }
    public float Width { get; private set; }
    public float X { get; set; }
    public float Y { get; set; }
    float _widthOffset;
    public void AddSpace(float width) => _widthOffset += width;

    public void Add(Display display, float? ascentOverride = null, float? descentOverride = null) {
      if (display.Ascent > Ascent) Ascent = ascentOverride ?? display.Ascent;
      if (display.Descent > Descent) Descent = descentOverride ?? display.Descent;
      display.Position =
        new System.Drawing.PointF(display.Position.X + Width + _widthOffset, display.Position.Y);
      Width += display.Width;
      _queue.Enqueue(display);
    }

    public void Clear(Action<Display> forEach) {
      for (int i = _queue.Count; i > 0; i--) {
        var display = _queue.Dequeue();
        display.Position =
        new System.Drawing.PointF(display.Position.X + X, display.Position.Y + Y);
        forEach(display);
      }
      _widthOffset = Ascent = Descent = Width = 0;
    }
  }
}