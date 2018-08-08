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
#warning ClampToPositive is a hack to offset Measure to a correct value, tested using Layout page of Forms.Example
    protected override RectangleF? MeasureCore(float canvasWidth) =>
      _relativeXCoordDisplay?.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft).ClampToPositive().Union(_absoluteXCoordDisplay.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft));
    public RectangleF? Measure(float canvasWidth) {
      UpdateDisplay(canvasWidth);
      return MeasureCore(canvasWidth);
    }

    protected override void UpdateDisplay(float canvasWidth) {
      if (Atom == null) return;
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0, firstLineAscent = 0;
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
          case TextAtom.Newline n:
            accumulatedHeight += lineHeight;
            lineWidth = lineHeight = 0;
            break;
          case TextAtom.Math m when m.DisplayStyle:
            accumulatedHeight += lineHeight;
#warning Replace 12 with a more appropriate spacing
            accumulatedHeight += 12;
            display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Display);
            if (color != null) display.SetTextColorRecursive(color);
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Bottom, default, default, default).X,
              -accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            accumulatedHeight += 12;
            lineWidth = lineHeight = 0;
            displayMathList.Add(display);
            break;
          default:
            RectangleF bounds;
            float width, height;
            switch (atom) {
              case TextAtom.Text t:
                var content = UnicodeFontChanger.Instance.ChangeFont(t.Content, style);
                var glyphs = GlyphFinder.Instance.FindGlyphs(fonts, content);
                float maxLineSpacing = 0;
#warning Optimise this
                foreach (var (typeface, _) in glyphs) {
                  float lineSpacing = Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(typeface) *
                    typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
                  if (lineSpacing > maxLineSpacing) maxLineSpacing = lineSpacing;
                }
                display = new Display.TextRunDisplay<Fonts, Glyph>(Display.Text.AttributedGlyphRuns.Create(content, glyphs, fonts, false), t.Range, TypesettingContext.Instance);
                bounds = display.ComputeDisplayBounds();
                width = bounds.Width;
                height = maxLineSpacing;
                break;
              case TextAtom.Math m:
                if (m.DisplayStyle) throw new InvalidCodePathException("Display style maths should have been handled above this switch.");
                display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
                bounds = display.ComputeDisplayBounds();
                width = bounds.Width;
                height = bounds.Height;
                break;
              case TextAtom.Style _:
              case TextAtom.Color _:
                throw new InvalidCodePathException("Style atoms and Color atoms should have been handled above this switch.");
              case var a:
                throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
            }
            if (lineWidth + display.Width > canvasWidth) {
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new PointF(0, -display.Position.Y-accumulatedHeight);
              lineWidth = width;
              lineHeight = height;
              if (firstLineAscent >= 0) firstLineAscent = System.Math.Max(firstLineAscent, display.Ascent);
            } else {
              lineHeight = System.Math.Max(lineHeight, height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new PointF(lineWidth, -display.Position.Y-accumulatedHeight);
              lineWidth += width;
              if (firstLineAscent > 0) firstLineAscent *= -1; //negate to freeze its value
            }
            if (color != null) display.SetTextColorRecursive(color);
            displayList.Add(display);
            break;
        }
      }
      var relativePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, Fonts, relativePositionList, absolutePositionList);
      if (firstLineAscent > 0) firstLineAscent *= -1;
      //Retain positions
      _relativeXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(relativePositionList);
      _absoluteXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(absolutePositionList) { Position = new PointF(0, _relativeXCoordDisplay.ComputeDisplayBounds(CoordinatesFromBottomLeftInsteadOfTopLeft).Y) };
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
