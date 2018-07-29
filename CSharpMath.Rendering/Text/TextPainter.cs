using System.Drawing;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }
    
    protected override IDisplay<Fonts, Glyph> CreateDisplay(float canvasWidth) {
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0;
      void AddDisplaysWithLineBreaks(TextAtom atom, System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList) {
        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content) AddDisplaysWithLineBreaks(a, displayList);
            break;
          case TextAtom.Newline n:
            accumulatedHeight += lineHeight;
            lineWidth = lineHeight = 0;
            break;
          case TextAtom.Math m when m.DisplayStyle:
            accumulatedHeight += lineHeight;
            accumulatedHeight += lineHeight;
            display = atom.ToDisplay(Fonts, default);
#warning This is affected by Painter's Draw method itself to offset to the right.
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, Fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Top, default, default, default).X,
              display.Position.Y - accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            lineWidth = lineHeight = 0;
            displayList.Add(display);
            break;
          default:
            display = atom.ToDisplay(Fonts, default);
            var bounds = display.ComputeDisplayBounds();
            if (lineWidth + display.Width > canvasWidth) {
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new PointF(0, display.Position.Y - accumulatedHeight);
              lineWidth = bounds.Width;
              lineHeight = bounds.Height;
            } else {
              lineHeight = System.Math.Max(lineHeight, bounds.Height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new PointF(lineWidth, -accumulatedHeight);
              lineWidth += bounds.Width;
            }
            displayList.Add(display);
            break;
        }
      }
      var returnList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, returnList);
      return new Display.MathListDisplay<Fonts, Glyph>(returnList);
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }

    public RectangleF? Measure(float canvasWidth) => MeasureCore(canvasWidth);

    public void Draw(TCanvas canvas) => Draw(WrapCanvas(canvas), default);
  }
}
