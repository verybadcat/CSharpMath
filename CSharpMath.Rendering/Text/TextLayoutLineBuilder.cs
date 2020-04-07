using System.Collections.Generic;

namespace CSharpMath.Rendering.Text {
  using Display = Display.IDisplay<BackEnd.Fonts, BackEnd.Glyph>;
  public class TextLayoutLineBuilder {
    readonly Queue<Display> _queue = new Queue<Display>();
    public float Ascent { get; private set; }
    public float Descent { get; private set; }
    public float Width { get; private set; }
    public float GapAfterLine { get; private set; }
    public void AddSpace(float width) => Width += width;
    public void Add(Display display, float ascender, float descender, float gapAfterLine) {
      static float Max(float x, float y, float z) =>
        x < y ? (y < z ? z : y) : (x < z ? z : x);
      Ascent = Max(Ascent, display.Ascent, ascender);
      Descent = Max(Descent, display.Descent, descender);
      GapAfterLine = gapAfterLine > GapAfterLine ? gapAfterLine : GapAfterLine;
      display.Position = new System.Drawing.PointF
        (display.Position.X + Width, display.Position.Y);
      Width += display.Width;
      _queue.Enqueue(display);
    }
    public void Clear(float x, float y, ICollection<Display> accumulator,
        ref float verticalAdvance, bool minusAscent, bool appendLineGap) {
      verticalAdvance += Ascent;
      for (int i = _queue.Count; i > 0; i--) {
        var display = _queue.Dequeue();
        //display.Position is display's bottom-left point, so minus Ascent
        display.Position = new System.Drawing.PointF
          (display.Position.X + x, display.Position.Y + (minusAscent ? y - Ascent : y));
        accumulator.Add(display);
      }
      verticalAdvance += Descent;
      if(appendLineGap) verticalAdvance += GapAfterLine;
      Ascent = Descent = Width = GapAfterLine = 0;
    }
  }
}