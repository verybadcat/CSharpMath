namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }

    /// <summary>
    /// Defaults to the width of the entire canvas.
    /// </summary>
    public float LineWidth { get => __width; set => Redisplay(__width = value); } float __width; 

    protected override IDisplay<MathFonts, Glyph> CreateDisplay(MathFonts fonts) {
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0;
      void AddDisplaysWithLineBreaks(TextAtom atom, System.Collections.Generic.List<IDisplay<MathFonts, Glyph>> displayList) {
        IDisplay<MathFonts, Glyph> display;
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
            display = atom.ToDisplay(fonts, default);
#warning This is affected by Painter's Draw method itself to offset to the right.
            display.Position = new System.Drawing.PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, fonts.PointSize, false, LineWidth, float.NaN, TextAlignment.Top, default, default, default).X,
              display.Position.Y - accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            lineWidth = lineHeight = 0;
            displayList.Add(display);
            break;
          default:
            display = atom.ToDisplay(fonts, default);
            var bounds = display.ComputeDisplayBounds();
            if (lineWidth + display.Width > LineWidth) {
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new System.Drawing.PointF(0, display.Position.Y - accumulatedHeight);
              lineWidth = bounds.Width;
              lineHeight = bounds.Height;
            } else {
              lineHeight = System.Math.Max(lineHeight, bounds.Height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new System.Drawing.PointF(lineWidth, -accumulatedHeight);
              lineWidth += bounds.Width;
            }
            displayList.Add(display);
            break;
        }
      }
      var returnList = new System.Collections.Generic.List<IDisplay<MathFonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, returnList);
      return new Display.MathListDisplay<MathFonts, Glyph>(returnList);
    }

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }
  }
}
