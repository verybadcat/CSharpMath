using System.Drawing;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }

    //display maths should always be center-aligned regardless of parameter for Draw()
    protected Display.MathListDisplay<Fonts, Glyph> _absoluteXCoordDisplay;
    protected Display.MathListDisplay<Fonts, Glyph> _relativeXCoordDisplay;
    protected Display.MathListDisplay<Fonts, Glyph> _combinedDisplay;

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.Text; set => Source = new TextSource(value); }

    protected override void SetRedisplay() { }
    protected override RectangleF? MeasureCore(float canvasWidth) =>
      _relativeXCoordDisplay?.ComputeDisplayBounds().Union(_absoluteXCoordDisplay.ComputeDisplayBounds());
    public RectangleF? Measure(float canvasWidth) {
      UpdateDisplay(canvasWidth);
      return MeasureCore(canvasWidth);
    }

    protected override void UpdateDisplay(float canvasWidth) {
      if (Atom == null) return;
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0, absYCoord = 0, relYCoord = 0;
      void AddDisplaysWithLineBreaks(TextAtom atom,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayMathList) {
        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content) AddDisplaysWithLineBreaks(a, displayList, displayMathList);
            break;
          case TextAtom.Newline n:
            accumulatedHeight += lineHeight;
            lineWidth = lineHeight = 0;
            break;
          case TextAtom.Math m when m.DisplayStyle:
            accumulatedHeight += lineHeight;
            display = atom.ToDisplay(Fonts);
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, Fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Bottom, default, default, default).X,
              -display.Ascent + display.Descent - accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            lineWidth = lineHeight = 0;
            displayMathList.Add(display);
            break;
          default:
            display = atom.ToDisplay(Fonts);
            var bounds = display.ComputeDisplayBounds();
            if (lineWidth + display.Width > canvasWidth) {
              //special case: first line's location should be below 0 (result of inverted canvas)
              if (accumulatedHeight == 0)
                absYCoord = relYCoord = -lineHeight;
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new PointF(0, display.Position.Y-accumulatedHeight);
              lineWidth = bounds.Width;
              lineHeight = bounds.Height;
            } else {
              lineHeight = System.Math.Max(lineHeight, bounds.Height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new PointF(lineWidth, display.Position.Y-accumulatedHeight);
              lineWidth += bounds.Width;
            }
            displayList.Add(display);
            break;
        }
      }
      var relativePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, relativePositionList, absolutePositionList);
      _absoluteXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(absolutePositionList) { Position = new PointF(0, absYCoord) };
      _relativeXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(relativePositionList) { Position = new PointF(0, relYCoord) };
    }

    public void Draw(TCanvas canvas) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(c.Width);
      Draw(c, new Display.MathListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay }));
    }
    public void Draw(TCanvas canvas, PointF position, float width) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(width);
      Draw(c, new Display.MathListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay }), position);
    }
    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.TopLeft, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(c.Width);
        _relativeXCoordDisplay.Position = _relativeXCoordDisplay.Position.Plus(IPainterExtensions.GetDisplayPosition(
          _relativeXCoordDisplay.Width, _relativeXCoordDisplay.Ascent, _relativeXCoordDisplay.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
        _absoluteXCoordDisplay.Position = new PointF(_absoluteXCoordDisplay.Position.X, _relativeXCoordDisplay.Position.Y);
        Draw(c, new Display.MathListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay }));
      }
    }
  }
}
