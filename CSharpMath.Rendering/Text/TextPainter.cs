using System.Drawing;
using System.Linq;

namespace CSharpMath.Rendering {
  public abstract class TextPainter<TCanvas, TColor> : Painter<TCanvas, TextSource, TColor> {
    public TextPainter(float fontSize = DefaultFontSize, float lineWidth = DefaultFontSize * 100) : base(fontSize) { }

    //display maths should always be center-aligned regardless of parameter for Draw()
    protected Display.MathListDisplay<Fonts, Glyph> _absoluteXCoordDisplay;
    protected Display.MathListDisplay<Fonts, Glyph> _relativeXCoordDisplay;
    protected Typography.TextLayout.GlyphLayout _glyphLayout = new Typography.TextLayout.GlyphLayout();

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
      float accumulatedHeight = 0, lineWidth = 0, lineHeight = 0;
      void AddDisplaysWithLineBreaks(TextAtom atom, Fonts fonts,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayList,
        System.Collections.Generic.List<IDisplay<Fonts, Glyph>> displayMathList,
        FontStyle style = FontStyle.Roman /*FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters*/) {
        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content) AddDisplaysWithLineBreaks(a, fonts, displayList, displayMathList, style);
            break;
          case TextAtom.Newline n:
            accumulatedHeight += lineHeight;
            lineWidth = lineHeight = 0;
            break;
          case TextAtom.Math m when m.DisplayStyle:
            accumulatedHeight += lineHeight;
            display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Display);
            display.Position = new PointF(
              IPainterExtensions.GetDisplayPosition(display.Width, display.Ascent, display.Descent, fonts.PointSize, false, canvasWidth, float.NaN, TextAlignment.Bottom, default, default, default).X,
              -accumulatedHeight);
            accumulatedHeight += display.Ascent + display.Descent;
            lineWidth = lineHeight = 0;
            displayMathList.Add(display);
            break;
          default:
            RectangleF bounds;
            float width, height;
            if (atom is TextAtom.Style st) { AddDisplaysWithLineBreaks(st.Content, fonts, displayList, displayMathList, st.FontStyle); return; }
            else if (atom is TextAtom.Size sz) { AddDisplaysWithLineBreaks(sz.Content, new Fonts(fonts, sz.PointSize), displayList, displayMathList, style); return; }
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
                display = Typesetter<Fonts, Glyph>.CreateLine(m.Content, fonts, TypesettingContext.Instance, Enumerations.LineStyle.Text);
                bounds = display.ComputeDisplayBounds();
                width = bounds.Width;
                height = bounds.Height;
                break;
              case TextAtom.Style _:
                throw new InvalidCodePathException("StyledText atoms should have been handled above this switch.");
              case var a:
                throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
            }
            if (lineWidth + display.Width > canvasWidth) {
              accumulatedHeight += lineHeight;
              //canvas inverted, so minus accumulatedHeight instead of plus
              display.Position = new PointF(0, display.Position.Y-accumulatedHeight);
              lineWidth = width;
              lineHeight = height;
            } else {
              lineHeight = System.Math.Max(lineHeight, height);
              //canvas inverted, so negate accumulatedHeight
              display.Position = new PointF(lineWidth, display.Position.Y-accumulatedHeight);
              lineWidth += width;
            }
            displayList.Add(display);
            break;
        }
      }
      var relativePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new System.Collections.Generic.List<IDisplay<Fonts, Glyph>>();
      AddDisplaysWithLineBreaks(Atom, Fonts, relativePositionList, absolutePositionList);
      _absoluteXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(absolutePositionList);
      _relativeXCoordDisplay = new Display.MathListDisplay<Fonts, Glyph>(relativePositionList);
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
