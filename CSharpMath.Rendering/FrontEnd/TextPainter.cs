using System.Drawing;
using System.Linq;

namespace CSharpMath.Rendering.FrontEnd {
  using BackEnd;
  using CSharpMath.Atom.Atoms;
  using Display;
  using Display.Displays;
  using Text;

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

    protected override Atom.Result<TextAtom> LaTeXToContent(string latex) =>
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
      DrawCore(canvas, null, null, alignment, padding, offsetX, offsetY, false, null);
#pragma warning disable RS0026 // RectangleF is a required, disambiguating second parameter.
    public void Draw(TCanvas canvas, RectangleF region,
      TextAlignment alignment = TextAlignment.TopLeft, Thickness padding = default,
      float offsetX = 0, float offsetY = 0) =>
      DrawCore(canvas, region.Width, region.Height, alignment, padding,
        region.X + offsetX, region.Y + offsetY, true, null);
#pragma warning restore RS0026
    public void Draw(TCanvas canvas, float top, float left, float right) =>
      DrawCore(canvas, right - left, null, TextAlignment.TopLeft, default, left, top, false, null);
    public void Draw(TCanvas canvas, PointF position, float width) =>
      DrawCore(canvas, width, null, TextAlignment.TopLeft, default, position.X, position.Y, false, null);
    internal void DrawAtLayoutWidth(TCanvas canvas, float layoutWidth,
      TextAlignment alignment = TextAlignment.TopLeft, float offsetX = 0, float offsetY = 0) =>
      DrawCore(canvas, null, null, alignment, default, offsetX, offsetY, false, layoutWidth);
    private void DrawCore(TCanvas canvas, float? width, float? height, TextAlignment alignment,
      Thickness padding, float offsetX, float offsetY, bool constrainCenteredInk,
      float? explicitLayoutWidth) {
      var c = WrapCanvas(canvas);
      var regionWidth = width ?? c.Width;
      // The public legacy overloads intentionally retain their original
      // geometry. Only the explicit finite region opts into constrained text.
      var constrained = constrainCenteredInk && !float.IsInfinity(regionWidth) && !float.IsNaN(regionWidth);
      var layoutWidth = explicitLayoutWidth ?? (constrained
        ? ConstrainedTextLayout.ContentWidth(regionWidth, padding.Left, padding.Right)
        : regionWidth);
      UpdateDisplay(layoutWidth);
      if (ErrorMessage == null) {
        var blockWidth = System.Math.Max(_relativeXCoordDisplay.Width, _absoluteXCoordDisplay.Width);
        var inkBeforePosition = DisplayInkBounds.GetInk(_relativeXCoordDisplay);
        var placementWidth = float.IsNaN(regionWidth) || float.IsInfinity(regionWidth)
          ? System.Math.Max(blockWidth, inkBeforePosition.Width + padding.Left + padding.Right)
          : regionWidth;
        _relativeXCoordDisplay.Position =
          _relativeXCoordDisplay.Position.Plus(IPainterExtensions.GetDisplayPosition(
            blockWidth,
            System.Math.Max(_relativeXCoordDisplay.Ascent, _absoluteXCoordDisplay.Ascent),
            System.Math.Max(_relativeXCoordDisplay.Descent, _absoluteXCoordDisplay.Descent),
             FontSize, placementWidth,
             FiniteHeight(height ?? c.Height, _relativeXCoordDisplay), alignment, padding, offsetX, offsetY
          ));
        var adjustedCanvasWidth =
          float.IsInfinity(c.Width) || float.IsNaN(c.Width)
          ? System.Math.Max(_relativeXCoordDisplay.Displays.CollectionWidth(),
            _absoluteXCoordDisplay.Displays.IsNonEmpty() ? _absoluteXCoordDisplay.Displays.Max(d => d.Width) : 0)
          : c.Width;
        // https://github.com/verybadcat/CSharpMath/issues/123
        // Take into account padding, offset etc. on both sides
        if (!constrained) {
          adjustedCanvasWidth -= _relativeXCoordDisplay.Position.X * 2;
          float Δx = 0;
          var y = float.NegativeInfinity;
          var leftRightFlags = alignment & (TextAlignment.Left | TextAlignment.Right);
          if (leftRightFlags == TextAlignment.Center)
            foreach (var relDisplay in _relativeXCoordDisplay.Displays.Reverse()) {
              if (relDisplay.Position.Y > y) {
                y = relDisplay.Position.Y;
                var rightSpace = adjustedCanvasWidth - (relDisplay.Position.X + relDisplay.Width);
                Δx = rightSpace / 2;
              }
              relDisplay.Position = new PointF(relDisplay.Position.X + Δx, y);
            }
          else if (leftRightFlags == TextAlignment.Right)
            foreach (var relDisplay in _relativeXCoordDisplay.Displays.Reverse()) {
              if (relDisplay.Position.Y > y) {
                y = relDisplay.Position.Y;
                var rightSpace = adjustedCanvasWidth - (relDisplay.Position.X + relDisplay.Width);
                Δx = rightSpace;
              }
              relDisplay.Position = new PointF(relDisplay.Position.X + Δx, y);
            }
        } else {
          var contentLeft = regionWidth == float.PositiveInfinity || float.IsNaN(regionWidth)
            ? _relativeXCoordDisplay.Position.X
            : offsetX + padding.Left;
          var contentRight = regionWidth == float.PositiveInfinity || float.IsNaN(regionWidth)
            ? adjustedCanvasWidth + contentLeft
            : offsetX + regionWidth - padding.Right;
          // GetDisplayPosition has already placed the outer display using
          // typographic centering. Derive the same local right-space formula
          // used by the legacy path so the constrained path does not center
          // that outer display a second time.
          float Δx = 0;
          var leftRightFlags = alignment & (TextAlignment.Left | TextAlignment.Right);
          if (leftRightFlags == TextAlignment.Center) {
            var y = float.NegativeInfinity;
            foreach (var relDisplay in _relativeXCoordDisplay.Displays.Reverse()) {
              if (relDisplay.Position.Y > y) {
                y = relDisplay.Position.Y;
                var lineDisplays = _relativeXCoordDisplay.Displays.Where(d => d.Position.Y == y).ToArray();
                var minInk = lineDisplays.Min(d => LineCenterBounds(d).Left + d.Position.X + _relativeXCoordDisplay.Position.X);
                var maxInk = lineDisplays.Max(d => LineCenterBounds(d).Right + d.Position.X + _relativeXCoordDisplay.Position.X);
                var rightSpace = blockWidth - (relDisplay.Position.X + relDisplay.Width);
                var oldShift = rightSpace / 2;
                Δx = CenterShift(oldShift, minInk, maxInk, contentLeft, contentRight);
              }
              relDisplay.Position = new PointF(relDisplay.Position.X + Δx, y);
            }
          } else if (leftRightFlags == TextAlignment.Right) {
            var y = float.NegativeInfinity;
            foreach (var relDisplay in _relativeXCoordDisplay.Displays.Reverse()) {
              if (relDisplay.Position.Y > y) {
                y = relDisplay.Position.Y;
                var rightSpace = blockWidth - (relDisplay.Position.X + relDisplay.Width);
                Δx = rightSpace;
              }
              relDisplay.Position = new PointF(relDisplay.Position.X + Δx, y);
            }
          } else if (leftRightFlags != TextAlignment.Left) {
            throw new Atom.InvalidCodePathException("The left flag has been set. This foreach loop should have been skipped.");
          }
        }
        static float CenterShift(float oldShift, float minInk, float maxInk,
          float contentLeft, float contentRight) {
          if (minInk + oldShift >= contentLeft && maxInk + oldShift <= contentRight)
            return oldShift;
          if (maxInk - minInk > contentRight - contentLeft)
            return oldShift;
          return System.Math.Max(contentLeft - minInk, System.Math.Min(contentRight - maxInk,
            (contentLeft + contentRight - minInk - maxInk) / 2));
        }
        static RectangleF LineCenterBounds(IDisplay<Fonts, Glyph> display) =>
          DisplayInkBounds.GetInk(display);
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
    static float FiniteHeight(float height, IDisplay<Fonts, Glyph> display) =>
      (!float.IsNaN(height) && !float.IsInfinity(height)) ? height : System.Math.Max(1, display.Ascent + display.Descent);
    /// <summary>
    /// Draws with respect to the only baseline which coordinates are given - center display maths with respect to text instead of canvas width.
    /// The measure of the result drawn by this method is NOT Measure(float.PositiveInfinity)
    /// as display maths, which is supposed to be centered, would be positioned at infinity for that call.
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
        new Atom.RentedArray<IDisplay<Fonts, Glyph>>(
          _relativeXCoordDisplay, _absoluteXCoordDisplay
        );
      DrawCore(c, new ListDisplay<Fonts, Glyph>(array.Result));
    }
    public new TextPainter<TCanvas, TColor> ShallowClone() => (TextPainter<TCanvas, TColor>)MemberwiseClone();
  }
}
