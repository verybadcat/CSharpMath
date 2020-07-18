using System.Drawing;
using System.Linq;

namespace CSharpMath.Rendering.FrontEnd {
  using Display;
  using Display.Displays;
  using BackEnd;
  using Structures;
  using Text;
  using CSharpMath.Atom.Atoms;

  /// <summary>
  /// Unlike <see cref="Typesetter{TFont, TGlyph}"/>,
  /// <see cref="TextPainter{TCanvas, TColor}"/>'s coordinates are inverted by default.
  /// </summary>
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextAtom, TColor> {
    public const float DefaultCanvasWidth = 2000f;
    public override IDisplay<Fonts, Glyph>? Display { get; protected set; }

    //display maths should always be center-aligned regardless of parameter for Draw()
    //so special case them into _absoluteXCoordDisplay instead of using _relativeXCoordDisplay
    public ListDisplay<Fonts, Glyph> _absoluteXCoordDisplay = new ListDisplay<Fonts, Glyph>(System.Array.Empty<IDisplay<Fonts, Glyph>>());
    public ListDisplay<Fonts, Glyph> _relativeXCoordDisplay = new ListDisplay<Fonts, Glyph>(System.Array.Empty<IDisplay<Fonts, Glyph>>());

    protected override Result<TextAtom> LaTeXToContent(string latex) =>
      TextLaTeXParser.TextAtomFromLaTeX(latex);
    protected override string ContentToLaTeX(TextAtom mathList) =>
      TextLaTeXParser.TextAtomToLaTeX(mathList).ToString();
    // Display has to be updated every draw as its position is mutated depending on canvas width
    protected override void SetRedisplay() { }
    protected override void UpdateDisplayCore(float canvasWidth) {
      if (ErrorMessage != null) {
        Display = null;
      } else {
        (_relativeXCoordDisplay, _absoluteXCoordDisplay) =
          TextTypesetter.Layout(Content ?? new TextAtom.List(System.Array.Empty<TextAtom>()), Fonts, canvasWidth);
        Display = new ListDisplay<Fonts, Glyph>(new[] { _relativeXCoordDisplay, _absoluteXCoordDisplay });
      }
    }

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
      UpdateDisplay(width ?? c.Width);
      if (ErrorMessage == null) {
        _relativeXCoordDisplay.Position =
          _relativeXCoordDisplay.Position.Plus(IPainterExtensions.GetDisplayPosition(
            System.Math.Max(_relativeXCoordDisplay.Width, _absoluteXCoordDisplay.Width),
            System.Math.Max(_relativeXCoordDisplay.Ascent, _absoluteXCoordDisplay.Ascent),
            System.Math.Max(_relativeXCoordDisplay.Descent, _absoluteXCoordDisplay.Descent),
            FontSize, width ?? c.Width,
            c.Height, alignment, padding, offsetX, offsetY
          ));
        var adjustedCanvasWidth =
          float.IsInfinity(c.Width) || float.IsNaN(c.Width)
          ? System.Math.Max(_relativeXCoordDisplay.Displays.CollectionWidth(),
            _absoluteXCoordDisplay.Displays.IsNonEmpty() ? _absoluteXCoordDisplay.Displays.Max(d => d.Width) : 0)
          : c.Width;
        // https://github.com/verybadcat/CSharpMath/issues/123
        // Take into account padding, offset etc. on both sides
        adjustedCanvasWidth -= _relativeXCoordDisplay.Position.X * 2;
        float Δx = 0;
        var y = float.NegativeInfinity;
        var leftRightFlags = alignment & (TextAlignment.Left | TextAlignment.Right);
        if (leftRightFlags != TextAlignment.Left)
          foreach (var relDisplay in _relativeXCoordDisplay.Displays.Reverse()) {
            if (relDisplay.Position.Y > y) {
              y = relDisplay.Position.Y;
              var rightSpace = adjustedCanvasWidth - (relDisplay.Position.X + relDisplay.Width);
              Δx = leftRightFlags switch
              {
                TextAlignment.Center => rightSpace / 2,
                TextAlignment.Right => rightSpace,
                _ => throw new InvalidCodePathException("The left flag has been set. This foreach loop should have been skipped.")
              };
            }
            relDisplay.Position = new PointF(relDisplay.Position.X + Δx, y);
          }
        //offsetY is already included in _relativeXCoordDisplay.Position,
        //no need to add it again below
        _absoluteXCoordDisplay.Position =
          new PointF(_absoluteXCoordDisplay.Position.X + offsetX,
                     _absoluteXCoordDisplay.Position.Y + _relativeXCoordDisplay.Position.Y);
        Display = new ListDisplay<Fonts, Glyph>(new[] {
           _relativeXCoordDisplay, _absoluteXCoordDisplay
        });
      }
      DrawCore(c, Display);
    }
    /// <summary>
    /// Draws with respect to the only baseline which coordinates are given.
    /// The measure of the result drawn by this method is NOT Measure(float.PositiveInfinity).
    /// </summary>
    public void DrawOneLine(TCanvas canvas, float x, float y) {
      var c = WrapCanvas(canvas);
      UpdateDisplay(float.PositiveInfinity);
      y -= _relativeXCoordDisplay.Displays.Max(dp => dp.Ascent);
      // Invert the canvas
      y *= -1;
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
