using System;
using System.Drawing;

using CSharpMath.Display;
using CSharpMath.Structures;

using Typography.OpenFont;
using System.Collections.Specialized;

namespace CSharpMath.Rendering.FrontEnd {
  using System.Collections.Generic;
  using System.Linq;
  using BackEnd;

  public static class PainterConstants {
    public const float DefaultFontSize = 14;
    public const float LargerFontSize = 50;
  }
  public abstract class Painter<TCanvas, TContent, TColor> : ICSharpMathAPI<TContent, TColor> where TContent : class {
    public const float DefaultFontSize = PainterConstants.DefaultFontSize;

    public Painter() {
      ErrorColor = UnwrapColor(Color.FromArgb(255, 0, 0));
      TextColor = UnwrapColor(Color.FromArgb(0, 0, 0));
      HighlightColor = UnwrapColor(Color.FromArgb(0, 0, 0, 0));
    }

    #region Non-redisplaying properties
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; } = true;
    public TColor ErrorColor { get; set; }
    public TColor TextColor { get; set; }
    public TColor HighlightColor { get; set; }
    public (TColor glyph, TColor textRun)? GlyphBoxColor { get; set; }
    public PaintStyle PaintStyle { get; set; } = PaintStyle.Fill;
    public float Magnification { get; set; } = 1;
    public string? ErrorMessage { get; protected set; }
    public abstract IDisplay<Fonts, Glyph>? Display { get; protected set; }
    #endregion Non-redisplaying properties

    #region Redisplaying properties
    //_field == private field, __field == property-only field
    protected abstract void SetRedisplay();
    protected Fonts Fonts { get; private set; } = new Fonts(Array.Empty<Typeface>(), DefaultFontSize);
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => Fonts.PointSize; set { Fonts = new Fonts(Fonts, value); SetRedisplay(); } }
    IEnumerable<Typeface> __localTypefaces = Array.Empty<Typeface>();
    public IEnumerable<Typeface> LocalTypefaces { get => __localTypefaces; set { Fonts = new Fonts(value, FontSize); __localTypefaces = value; SetRedisplay(); } }
    Atom.LineStyle __style = Atom.LineStyle.Display;
    public Atom.LineStyle LineStyle { get => __style; set { __style = value; SetRedisplay(); } }
    TContent? __content;
    public TContent? Content { get => __content; set { __content = value; SetRedisplay(); } }
    public string? LaTeX { get => Content is null ? "" : ContentToLaTeX(Content); set => (Content, ErrorMessage) = LaTeXToContent(value ?? ""); }
    #endregion Redisplaying properties

    #region Methods
    protected abstract Result<TContent> LaTeXToContent(string latex);
    protected abstract string ContentToLaTeX(TContent content);
    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);
    public abstract ICanvas WrapCanvas(TCanvas canvas);
    public virtual RectangleF Measure(float textPainterCanvasWidth) {
      UpdateDisplay(textPainterCanvasWidth);
      if (Display != null)
        return new RectangleF(0, -Display.Ascent, Display.Width, Display.Ascent + Display.Descent);
      else return RectangleF.Empty;
    }
    protected abstract void UpdateDisplayCore(float textPainterCanvasWidth);
    protected void UpdateDisplay(float textPainterCanvasWidth) {
      UpdateDisplayCore(textPainterCanvasWidth);
      if (Display == null && DisplayErrorInline && ErrorMessage != null) {
        var font = Fonts;
        if (ErrorFontSize is { } errorSize) font = new Fonts(font, errorSize);
        var errorLines = ErrorMessage.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var runs = new List<Display.Displays.TextRunDisplay<Fonts, Glyph>>();
        float y = 0;
        for (var i = 0; i < errorLines.Length; i++) {
          var errorLine = errorLines[i];
          float x = 0;
          if (i == errorLines.Length - 1 && errorLines.Length > 1) {
            var pointer = errorLine.TrimStart(' ');
            var spaces = errorLine.Length - pointer.Length;
            var pointerIndentChars = errorLines[i - 1];
            if (spaces < pointerIndentChars.Length)
              pointerIndentChars = pointerIndentChars.Remove(spaces);
            x =
              TypesettingContext.Instance.GlyphBoundsProvider.GetTypographicWidth(font,
                new AttributedGlyphRun<Fonts, Glyph>(pointerIndentChars,
                TypesettingContext.Instance.GlyphFinder.FindGlyphs(font, pointerIndentChars),
                font));
            errorLine = pointer;
          }
          var run = new Display.Displays.TextRunDisplay<Fonts, Glyph>(
                new AttributedGlyphRun<Fonts, Glyph>(errorLine,
                TypesettingContext.Instance.GlyphFinder.FindGlyphs(font, errorLine),
                font),
              Atom.Range.Zero, TypesettingContext.Instance);
          run.SetTextColorRecursive(WrapColor(ErrorColor));
          y -= run.Ascent;
          run.Position = new PointF(x, y);
          y -= run.Descent
             + run.Run.Glyphs.Max(g => g.Typeface.LineGap * g.Typeface.CalculateScaleToPixelFromPointSize(font.PointSize));
          runs.Add(run);
        }
        Display = new Display.Displays.TextLineDisplay<Fonts, Glyph>(runs, Array.Empty<Atom.MathAtom>(), default);
        Display.SetTextColorRecursive(WrapColor(ErrorColor));
      }
    }
    public abstract void Draw(TCanvas canvas, TextAlignment alignment, Thickness padding = default, float offsetX = 0, float offsetY = 0);
    protected void DrawCore(ICanvas canvas, IDisplay<Fonts, Glyph>? display, PointF? position = null) {
      if (display != null) {
        canvas.Save();
        //invert the canvas vertically: displays are drawn with mathematical coordinates, not graphical coordinates
        canvas.Scale(1, -1);
        canvas.Scale(Magnification, Magnification);
        if (position is { } p) display.Position = new PointF(p.X, p.Y);
        canvas.DefaultColor = WrapColor(TextColor);
        canvas.CurrentColor = WrapColor(HighlightColor);
        canvas.CurrentStyle = PaintStyle;
        var measure = Measure(canvas.Width);
        canvas.FillRect(display.Position.X + measure.X, display.Position.Y - display.Descent,
          measure.Width, measure.Height);
        canvas.CurrentColor = null;
        static T? Nullable<T>(T nonnull) where T : struct => new T?(nonnull);
        display.Draw(new GraphicsContext(canvas,
          GlyphBoxColor is var (glyph, textRun) ? Nullable((WrapColor(glyph), WrapColor(textRun))) : null
        ));
        canvas.Restore();
      }
    }
    #endregion Methods
  }
}