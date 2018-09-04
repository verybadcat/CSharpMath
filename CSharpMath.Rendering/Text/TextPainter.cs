using System.Drawing;
using System.Linq;

namespace CSharpMath.Rendering {
  /// <summary>
  /// Unlike <see cref="CSharpMath.Typesetter{TFont, TGlyph}"/>, <see cref="TextPainter{TCanvas, TColor}"/>'s coordinates are inverted by default.
  /// </summary>
  /// <typeparam name="TCanvas"></typeparam>
  /// <typeparam name="TColor"></typeparam>
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }

    //display maths should always be center-aligned regardless of parameter for Draw()
    public Display.MathListDisplay<Fonts, Glyph> _absoluteXCoordDisplay;
    public Display.MathListDisplay<Fonts, Glyph> _relativeXCoordDisplay;
    protected Typography.TextLayout.GlyphLayout _glyphLayout = new Typography.TextLayout.GlyphLayout();

    public TextAtom Atom { get => Source.Atom; set => Source = new TextSource(value); }
    public string Text { get => Source.LaTeX; set => Source = new TextSource(value); }
    public float AdditionalLineSpacing { get; set; }

    protected override void SetRedisplay() { }
    protected override RectangleF? MeasureCore(float canvasWidth) =>
      _relativeXCoordDisplay?.Frame(CoordinatesFromBottomLeftInsteadOfTopLeft).Union(_absoluteXCoordDisplay.Frame(CoordinatesFromBottomLeftInsteadOfTopLeft));
    public RectangleF? Measure(float canvasWidth) {
      UpdateDisplay(canvasWidth);
      return MeasureCore(canvasWidth);
    }

    protected void UpdateDisplay(float canvasWidth) =>
      (_relativeXCoordDisplay, _absoluteXCoordDisplay) = TextLayoutter.Layout(Atom, Fonts, canvasWidth, AdditionalLineSpacing);

    public override void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.TopLeft, Thickness padding = default, float offsetX = 0, float offsetY = 0) =>
      DrawCore(canvas, null, alignment, padding, offsetX, offsetY);
    public void Draw(TCanvas canvas, float top, float left, float right) =>
      DrawCore(canvas, right - left, TextAlignment.TopLeft, default, left, top);
    public void Draw(TCanvas canvas, PointF position, float width) =>
      DrawCore(canvas, width, TextAlignment.TopLeft, default, position.X, position.Y);
    private void DrawCore(TCanvas canvas, float? width, TextAlignment alignment, Thickness padding, float offsetX, float offsetY) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(width ?? c.Width);
        _relativeXCoordDisplay.Position = _relativeXCoordDisplay.Position.Plus(IPainterExtensions.GetDisplayPosition(
          _relativeXCoordDisplay.Width, _relativeXCoordDisplay.Ascent, _relativeXCoordDisplay.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, width ?? c.Width, c.Height, alignment, padding, offsetX, offsetY));
        //offsetY is already included in _relativeXCoordDisplay.Position, no need to add it again below
        _absoluteXCoordDisplay.Position = new PointF(_absoluteXCoordDisplay.Position.X + offsetX, _absoluteXCoordDisplay.Position.Y + _relativeXCoordDisplay.Position.Y);
        DrawCore(c, new Display.MathListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay }));
      }
    }
    /// <summary>
    /// Draws with respect to the only baseline which coordinates are given. The measure of the result drawn by this method is NOT Measure(float.PositiveInfinity).
    /// </summary>
    public void DrawOneLine(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      if (!Source.IsValid) DrawError(c);
      else {
        UpdateDisplay(float.PositiveInfinity);
        if (!CoordinatesFromBottomLeftInsteadOfTopLeft) { y -= _relativeXCoordDisplay.Descent; y *= -1; } else y -= _relativeXCoordDisplay.Ascent;
        _relativeXCoordDisplay.Position = new PointF(_relativeXCoordDisplay.Position.X + x, _relativeXCoordDisplay.Position.Y + y);
        //y is already included in _relativeXCoordDisplay.Position, no need to add it again below
        _absoluteXCoordDisplay.Position = new PointF(_absoluteXCoordDisplay.Position.X + x, _relativeXCoordDisplay.Position.Y);
        DrawCore(c, new Display.MathListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay }));
      }
    }
  }
}
