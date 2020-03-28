using System.Drawing;
using System.Linq;

namespace CSharpMath.Rendering.FrontEnd {
  using Display;
  using Display.Displays;
  using BackEnd;
  using Structures;
  using Text;
  /// <summary>
  /// Unlike <see cref="Typesetter{TFont, TGlyph}"/>,
  /// <see cref="TextPainter{TCanvas, TColor}"/>'s coordinates are inverted by default.
  /// </summary>
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextAtom, TColor> {
    public const float DefaultCanvasWidth = 2000f;
    public override IDisplay<Fonts, Glyph> Display =>
      new ListDisplay<Fonts, Glyph>(new[] {
        _relativeXCoordDisplay, _absoluteXCoordDisplay
      });

    //display maths should always be center-aligned regardless of parameter for Draw()
    //so special case them into _absoluteXCoordDisplay instead of using _relativeXCoordDisplay
    public ListDisplay<Fonts, Glyph> _absoluteXCoordDisplay = new ListDisplay<Fonts, Glyph>(System.Array.Empty<IDisplay<Fonts, Glyph>>());
    public ListDisplay<Fonts, Glyph> _relativeXCoordDisplay = new ListDisplay<Fonts, Glyph>(System.Array.Empty<IDisplay<Fonts, Glyph>>());
    public override string LaTeX {
      get => Content is null ? "" : TextLaTeXBuilder.TextAtomToLaTeX(Content).ToString();
      set => (Content, ErrorMessage) = TextLaTeXBuilder.TextAtomFromLaTeX(value);
    }
    public float AdditionalLineSpacing { get; set; }

    protected override void SetRedisplay() { }
    public override RectangleF? Measure(float canvasWidth) {
      if (float.IsInfinity(canvasWidth) || float.IsNaN(canvasWidth))
        canvasWidth = DefaultCanvasWidth; // Rationalize the input
      UpdateDisplay(canvasWidth);
      return 
        _relativeXCoordDisplay?.Frame().Union(_absoluteXCoordDisplay.Frame());
    }
    protected void UpdateDisplay(float canvasWidth) =>
      (_relativeXCoordDisplay, _absoluteXCoordDisplay) =
        TextLayoutter.Layout(Content, Fonts, canvasWidth, AdditionalLineSpacing);
    public override void Draw(TCanvas canvas,
        TextAlignment alignment = TextAlignment.TopLeft, Thickness padding = default,
        float offsetX = 0, float offsetY = 0) =>
      DrawCore(canvas, null, alignment, padding, offsetX, offsetY);
    public void Draw(TCanvas canvas, float top, float left, float right) =>
      DrawCore(canvas, right - left, TextAlignment.TopLeft, default, left, top);
    public void Draw(TCanvas canvas, PointF position, float width) =>
      DrawCore(canvas, width, TextAlignment.TopLeft, default, position.X, position.Y);
    private void DrawCore(TCanvas canvas, float? width, TextAlignment alignment,
      Thickness padding, float offsetX, float offsetY) {
      var c = WrapCanvas(canvas);
      if (Content is null) DrawError(c);
      else {
        UpdateDisplay(width ?? c.Width);
        _relativeXCoordDisplay.Position =
          _relativeXCoordDisplay.Position.Plus(IPainterExtensions.GetDisplayPosition(
            _relativeXCoordDisplay.Width, _relativeXCoordDisplay.Ascent,
            _relativeXCoordDisplay.Descent, FontSize,
            CoordinatesFromBottomLeftInsteadOfTopLeft, width ?? c.Width,
            c.Height, alignment, padding, offsetX, offsetY
          ));
        //offsetY is already included in _relativeXCoordDisplay.Position,
        //no need to add it again below
        _absoluteXCoordDisplay.Position =
          new PointF(_absoluteXCoordDisplay.Position.X + offsetX,
                     _absoluteXCoordDisplay.Position.Y + _relativeXCoordDisplay.Position.Y);
        using var array = new RentedArray<IDisplay<Fonts, Glyph>>(
          _relativeXCoordDisplay, _absoluteXCoordDisplay
        );
        DrawCore(c, new ListDisplay<Fonts, Glyph>(array.Result));
      }
    }
    /// <summary>
    /// Draws with respect to the only baseline which coordinates are given.
    /// The measure of the result drawn by this method is NOT Measure(float.PositiveInfinity).
    /// </summary>
    public void DrawOneLine(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      if (Content is null) DrawError(c);
      else {
        UpdateDisplay(float.PositiveInfinity);
        if (!CoordinatesFromBottomLeftInsteadOfTopLeft) {
          y -= _relativeXCoordDisplay.Displays.Max(dp => dp.Ascent);
          y *= -1;
        } else y -= _relativeXCoordDisplay.Displays.Max(dp => dp.Descent);
        _relativeXCoordDisplay.Position =
          new PointF(_relativeXCoordDisplay.Position.X + x,
                     _relativeXCoordDisplay.Position.Y + y);
        //y is already included in _relativeXCoordDisplay.Position,
        //no need to add it again below
        _absoluteXCoordDisplay.Position =
          new PointF(_absoluteXCoordDisplay.Position.X + x,
                     _relativeXCoordDisplay.Position.Y);
        using var array =
          new RentedArray<IDisplay<Fonts, Glyph>>(
            _relativeXCoordDisplay, _absoluteXCoordDisplay
          );
        DrawCore(c, new ListDisplay<Fonts, Glyph>(array.Result));
      }
    }
  }
}
