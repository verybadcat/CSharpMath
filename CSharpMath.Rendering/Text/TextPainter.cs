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
    public string Text { get => Source.Text; set => Source = new TextSource(value); }

    protected override void SetRedisplay() { }
    protected override RectangleF? MeasureCore(float canvasWidth) =>
      _relativeXCoordDisplay?.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft).Union(_absoluteXCoordDisplay.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft));
    public RectangleF? Measure(float canvasWidth) {
      UpdateDisplay(canvasWidth);
      return MeasureCore(canvasWidth);
    }

    protected override void UpdateDisplay(float canvasWidth) {
      if (Atom == null) return;
      float accumulatedHeight = 0;
      TextDisplayLineBuilder line = new TextDisplayLineBuilder();
      void BreakLine(System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList) {
        accumulatedHeight += line.Ascent;
        line.Clear(0, -accumulatedHeight, displayList.Add, () => accumulatedHeight += line.Descent);
      }
      void AddDisplaysWithLineBreaks(TextAtom atom, Fonts fonts,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayMathList,
        FontStyle style = FontStyle.Roman, /*FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters*/
        Structures.Color? color = null) {

        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content) AddDisplaysWithLineBreaks(a, fonts, displayList, displayMathList, style, color);
            break;
          case TextAtom.Style st:
            AddDisplaysWithLineBreaks(st.Content, fonts, displayList, displayMathList, st.FontStyle, color);
            break;
          case TextAtom.Size sz:
            AddDisplaysWithLineBreaks(sz.Content, new Fonts(fonts, sz.PointSize), displayList, displayMathList, style, color);
            break;
          case TextAtom.Color c:
            AddDisplaysWithLineBreaks(c.Content, fonts, displayList, displayMathList, style, c.Colour);
            break;
          case TextAtom.Space sp:
            //Allow space at start of line since user explicitly specified its length
            //Also \par generates this kind of spaces
            line.AddSpace(sp.Content.ActualLength(MathTable.Instance, fonts));
            break;
          case TextAtom.Newline n:
            BreakLine(displayList);
            break;
          case TextAtom.Math m when m.DisplayStyle:
            BreakLine(displayList);
#warning Replace 12 with a more appropriate spacing
            accumulatedHeight += 12;
            display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Display);
            if (color != null) display.SetTextColorRecursive(color);
            accumulatedHeight += display.Ascent;
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Top, default, default, default).X,
              -accumulatedHeight);
            accumulatedHeight += display.Descent;
            accumulatedHeight += 12;
            if (color != null) display.SetTextColorRecursive(color);
            displayMathList.Add(display);
            break;

            void FinalizeInlineDisplay(float ascentMin = 0, bool forbidAtLineStart = false) {
              if (color != null) display.SetTextColorRecursive(color);
              if (line.Width + display.Width > canvasWidth && !forbidAtLineStart)
                BreakLine(displayList);
              line.Add(display, ascentMin);
            }
          case TextAtom.Text t:
            var content = UnicodeFontChanger.Instance.ChangeFont(t.Content, style);
            var glyphs = GlyphFinder.Instance.FindGlyphs(fonts, content);
            float maxLineSpacing = glyphs.Select(g => g.Typeface).Distinct().Select(tf =>
              Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(tf) *
              tf.CalculateScaleToPixelFromPointSize(fonts.PointSize)
            ).Max();
            display = new Display.TextRunDisplay<Fonts, Glyph>(Display.Text.AttributedGlyphRuns.Create(content, glyphs, fonts, false), t.Range, TypesettingContext.Instance);
            FinalizeInlineDisplay(maxLineSpacing);
            break;
          case TextAtom.Math m:
            if (m.DisplayStyle) throw new InvalidCodePathException("Display style maths should have been handled above this switch.");
            display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
            FinalizeInlineDisplay();
            break;
          case TextAtom.ControlSpace cs:
            display = new Display.TextRunDisplay<Fonts, Glyph>(Display.Text.AttributedGlyphRuns.Create(" ", new[] { GlyphFinder.Instance.Lookup(fonts, ' ') }, fonts, false), cs.Range, TypesettingContext.Instance);
            FinalizeInlineDisplay(forbidAtLineStart: true); //No spaces at start of line
            break;
          case null:
            throw new System.InvalidOperationException("TextAtoms should never be null. You must have sneaked one in.");
          case var a:
            throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
        }
      }
      var relativePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, Fonts, relativePositionList, absolutePositionList);
      BreakLine(relativePositionList);
      //Retain positions
      _relativeXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(relativePositionList);
      _absoluteXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(absolutePositionList);
    }

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
  }
}
