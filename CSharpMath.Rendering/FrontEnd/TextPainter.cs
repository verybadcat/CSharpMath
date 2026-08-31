using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

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

    TextDirection _textDirection = TextDirection.LeftToRight;

    /// <summary>Gets or sets the base direction used by <see cref="BidiParagraphs"/>.</summary>
    /// <remarks>
    /// The default is <see cref="Text.TextDirection.LeftToRight"/> for compatibility. This setting
    /// is independent of the <see cref="TextAlignment"/> supplied to drawing methods. It currently
    /// changes ordering metadata only; drawing remains in logical order.
    /// </remarks>
    public TextDirection TextDirection {
      get => _textDirection;
      set => _textDirection = BidiParagraph.ValidateDirection(value);
    }

    /// <summary>
    /// Gets freshly resolved paragraphs for a synthetic logical-text projection of <see cref="Content"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every paragraph and run index is a global UTF-16 offset into that projection. It is not an
    /// index into the LaTeX source or the <see cref="TextAtom"/> tree.
    /// </para>
    /// <para>
    /// <see cref="TextAtom.List"/> concatenates child projections in logical order.
    /// <see cref="TextAtom.Style"/>, <see cref="TextAtom.Size"/>, <see cref="TextAtom.Colored"/>, and
    /// <see cref="TextAtom.Accent"/> recursively project their content. Both
    /// <see cref="TextAtom.Space"/> and <see cref="TextAtom.ControlSpace"/> project to U+0020;
    /// <see cref="TextAtom.Newline"/> projects to CRLF; and <see cref="TextAtom.Math"/> projects to
    /// U+FFFC OBJECT REPLACEMENT CHARACTER. Comments, null content, and unknown atom types are
    /// omitted.
    /// </para>
    /// <para>
    /// The returned graph is an immutable snapshot and is recomputed on every access. Resolved
    /// levels come from the already-vendored Typography.TextBreak.SheenBidi implementation.
    /// Visual reordering and shaping are deferred to issues #290 and #291.
    /// </para>
    /// </remarks>
    public IReadOnlyList<BidiParagraph> BidiParagraphs =>
      BidiResolver.ResolveParagraphs(ContentToBidiText(Content), TextDirection);

    static string ContentToBidiText(TextAtom? atom) {
      var result = new StringBuilder();
      AppendBidiText(result, atom);
      return result.ToString();
    }

    static void AppendBidiText(StringBuilder result, TextAtom? atom) {
      switch (atom) {
        case TextAtom.Text text:
          result.Append(text.Content);
          break;
        case TextAtom.Newline:
          result.Append("\r\n");
          break;
        case TextAtom.Space:
        case TextAtom.ControlSpace:
          result.Append(' ');
          break;
        case TextAtom.Math:
          result.Append('\uFFFC');
          break;
        case TextAtom.Style style:
          AppendBidiText(result, style.Content);
          break;
        case TextAtom.Size size:
          AppendBidiText(result, size.Content);
          break;
        case TextAtom.Colored colored:
          AppendBidiText(result, colored.Content);
          break;
        case TextAtom.Accent accent:
          AppendBidiText(result, accent.Content);
          break;
        case TextAtom.List list:
          foreach (var child in list.Content)
            AppendBidiText(result, child);
          break;
      }
    }

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
              Δx = leftRightFlags switch {
                TextAlignment.Center => rightSpace / 2,
                TextAlignment.Right => rightSpace,
                _ => throw new Atom.InvalidCodePathException("The left flag has been set. This foreach loop should have been skipped.")
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
