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
    float _widthOffset;
    public void AddSpace(float width) => _widthOffset += width;

    public void Add(Display display, float ascentMin = 0) {
      var ascent = Math.Max(display.Ascent, ascentMin);
      if (ascent > Ascent) Ascent = ascent;
      if (display.Descent > Descent) Descent = display.Descent;
      display.Position =
        new System.Drawing.PointF(display.Position.X + Width + _widthOffset, display.Position.Y);
      Width += display.Width;
      _queue.Enqueue(display);
    }

    public void Clear(float x, float y, Action<Display> forEach, Action end) {
      for (int i = _queue.Count; i > 0; i--) {
        var display = _queue.Dequeue();
        display.Position =
        new System.Drawing.PointF(display.Position.X + x, display.Position.Y + y);
        forEach(display);
      }
      end();
      _widthOffset = Ascent = Descent = Width = 0;
    }
  }
}