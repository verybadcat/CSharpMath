using System;
using System.Collections.Specialized;
using System.Drawing;
using CSharpMath.Display;
using Typography.OpenFont;

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
    protected abstract Atom.Result<TContent> LaTeXToContent(string latex);
    protected abstract string ContentToLaTeX(TContent content);
    public abstract Color WrapColor(TColor color);
    public abstract TColor UnwrapColor(Color color);
    public abstract ICanvas WrapCanvas(TCanvas canvas);
    public virtual RectangleF Measure(float textPainterCanvasWidth) {
      UpdateDisplay(textPainterCanvasWidth);
      if (Display == null) return RectangleF.Empty;
      if (!DisplayInkBounds.RequiresAggregateBounds(Display))
        return new RectangleF(0, -Display.Ascent,
          Display.Width, Display.Ascent + Display.Descent);
      var horizontalBounds = DisplayInkBounds.Get(Display);
      return new RectangleF(horizontalBounds.Left, -Display.Ascent,
        horizontalBounds.Width, Display.Ascent + Display.Descent);
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
        // The display has already been laid out by the caller. Calling the
        // virtual Measure here would relayout TextPainter displays at the
        // surface width and change their geometry while drawing.
        var aggregateBounds = DisplayInkBounds.RequiresAggregateBounds(display);
        var measure = aggregateBounds
          ? DisplayInkBounds.Get(display)
          : Measure(canvas.Width);
        if (aggregateBounds)
          canvas.FillRect(display.Position.X + measure.X, display.Position.Y + measure.Y,
            measure.Width, measure.Height);
        else
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
    public Painter<TCanvas, TContent, TColor> ShallowClone() => (Painter<TCanvas, TContent, TColor>)MemberwiseClone();
    #endregion Methods
  }

  internal static class DisplayInkBounds {
    public static RectangleF GetInk(IDisplay<Fonts, Glyph> display) => GetCore(display, false);
    public static RectangleF Get(IDisplay<Fonts, Glyph> display) {
      return GetCore(display, true);
    }
    public static RectangleF GetTypographic(IDisplay<Fonts, Glyph> display) =>
      GetCore(display, true, true);
    public static bool ExtendsOwnAdvance(IDisplay<Fonts, Glyph> display) {
      var bounds = GetTypographic(display);
      return bounds.Left < -0.01f || bounds.Right > display.Width + 0.01f;
    }
    public static bool RequiresAggregateBounds(IDisplay<Fonts, Glyph> display) {
      if (!ContainsMultipleRows(display)) return false;
      var bounds = GetTypographic(display);
      return bounds.Left < -0.01f || bounds.Right > display.Width + 0.01f;
    }
    public static bool ContainsMultipleRows(IDisplay<Fonts, Glyph> display) {
      if (display is Display.Displays.ListDisplay<Fonts, Glyph> list) {
        var rows = list.Displays
          .OfType<Display.Displays.ListDisplay<Fonts, Glyph>>()
          .Where(row => row.LinePosition == Display.LinePosition.Regular)
          .ToArray();
        if (rows.Length > 1 && rows.Any(row =>
          System.Math.Abs(row.Position.Y - rows[0].Position.Y) > 0.01f)) return true;
        return list.Displays.Any(ContainsMultipleRows);
      }
      if (display is Display.Displays.TextLineDisplay<Fonts, Glyph> line)
        return line.Runs.Any(ContainsMultipleRows);
      if (display is Display.Displays.InnerDisplay<Fonts, Glyph> inner)
        return ContainsMultipleRows(inner.Inner);
      if (display is Display.Displays.FractionDisplay<Fonts, Glyph> fraction)
        return ContainsMultipleRows(fraction.Numerator) || ContainsMultipleRows(fraction.Denominator);
      if (display is Display.Displays.RadicalDisplay<Fonts, Glyph> radical)
        return ContainsMultipleRows(radical.Radicand)
          || (radical.Degree != null && ContainsMultipleRows(radical.Degree));
      if (display is Display.Displays.AccentDisplay<Fonts, Glyph> accent)
        return ContainsMultipleRows(accent.Accentee);
      if (display is Display.Displays.LargeOpLimitsDisplay<Fonts, Glyph> limits)
        return ContainsMultipleRows(limits.NucleusDisplay)
          || (limits.UpperLimit != null && ContainsMultipleRows(limits.UpperLimit))
          || (limits.LowerLimit != null && ContainsMultipleRows(limits.LowerLimit));
      if (display is Display.Displays.OverOrUnderlineDisplay<Fonts, Glyph> overUnder)
        return ContainsMultipleRows(overUnder.Inner);
      if (display is Display.Displays.UnderAnnotationDisplay<Fonts, Glyph> annotation)
        return ContainsMultipleRows(annotation.Inner) || ContainsMultipleRows(annotation.UnderList);
      return false;
    }
    static RectangleF GetCore(IDisplay<Fonts, Glyph> display, bool includeOwn,
      bool typographicOnly = false) {
      if (display is Display.Displays.TextRunDisplay<Fonts, Glyph> run) {
        if (typographicOnly) return display.DisplayBounds();
        if (!includeOwn) return run.InkBounds;
        var bounds = display.DisplayBounds();
        return run.InkBounds.IsEmpty ? bounds : bounds.Union(run.InkBounds);
      }
      if (display is Display.Displays.TextLineDisplay<Fonts, Glyph> line) {
        return WithChildren(line, line.Runs, false, includeOwn, false, typographicOnly);
      }
      if (display is Display.Displays.ListDisplay<Fonts, Glyph> list)
        return WithChildren(list, list.Displays, false, includeOwn, false, typographicOnly);
      if (display is Display.Displays.InnerDisplay<Fonts, Glyph> inner)
        return WithChildren(inner, new IDisplay<Fonts, Glyph>[] { inner.Left, inner.Inner, inner.Right }.Where(d => d != null)!, includeOwn, includeOwn, true, typographicOnly);
      if (display is Display.Displays.AccentDisplay<Fonts, Glyph> accent) {
        var bounds = includeOwn ? display.DisplayBounds() : RectangleF.Empty;
        var accentee = GetCore(accent.Accentee, includeOwn, typographicOnly);
        if (!accentee.IsEmpty) {
          var offset = new PointF(accent.Accentee.Position.X - accent.Position.X,
                                  accent.Accentee.Position.Y - accent.Position.Y);
          bounds = bounds.IsEmpty ? accentee.Plus(offset) : bounds.Union(accentee.Plus(offset));
        }
        var glyph = GetCore(accent.Accent, includeOwn, typographicOnly);
        if (!glyph.IsEmpty)
          bounds = bounds.IsEmpty ? glyph.Plus(accent.Accent.Position) : bounds.Union(glyph.Plus(accent.Accent.Position));
        return bounds.IsEmpty && includeOwn ? display.DisplayBounds() : bounds;
      }
      if (display is Display.Displays.FractionDisplay<Fonts, Glyph> fraction)
        return WithChildren(fraction, new[] { fraction.Numerator, fraction.Denominator }, includeOwn, includeOwn, true, typographicOnly);
      if (display is Display.Displays.RadicalDisplay<Fonts, Glyph> radical)
        return WithChildren(radical, new IDisplay<Fonts, Glyph>[] { radical.Radicand, radical.Degree }.Where(d => d != null)!, includeOwn, includeOwn, true, typographicOnly);
      if (display is Display.Displays.LargeOpLimitsDisplay<Fonts, Glyph> limits)
        return WithChildren(limits, new IDisplay<Fonts, Glyph>[] { limits.NucleusDisplay, limits.UpperLimit, limits.LowerLimit }.Where(d => d != null)!, includeOwn, includeOwn, true, typographicOnly);
      if (display is Display.Displays.OverOrUnderlineDisplay<Fonts, Glyph> overUnder)
        return WithChildren(overUnder, new[] { overUnder.Inner }, includeOwn, includeOwn, true, typographicOnly);
      if (display is Display.Displays.UnderAnnotationDisplay<Fonts, Glyph> annotation)
        return WithChildren(annotation, new IDisplay<Fonts, Glyph>[] { annotation.Inner, annotation.UnderList, annotation.AnnotationGlyph }.Where(d => d != null)!, includeOwn, includeOwn, true, typographicOnly);
      return display.DisplayBounds();
    }

    static RectangleF WithChildren(IDisplay<Fonts, Glyph> display,
      IEnumerable<IDisplay<Fonts, Glyph>?> children,
      bool includeContainer, bool includeChildLayoutBounds,
      bool normalizeChildPositions, bool typographicOnly = false) {
      var bounds = includeContainer ? display.DisplayBounds() : RectangleF.Empty;
      foreach (var child in children) {
        if (child == null) continue;
        var childBounds = GetCore(child, includeChildLayoutBounds, typographicOnly);
        if (!childBounds.IsEmpty) {
          var childPosition = normalizeChildPositions
            ? new PointF(child.Position.X - display.Position.X,
                         child.Position.Y - display.Position.Y)
            : child.Position;
          var positioned = childBounds.Plus(childPosition);
          bounds = bounds.IsEmpty ? positioned : bounds.Union(positioned);
        }
      }
      return bounds.IsEmpty && includeChildLayoutBounds ? display.DisplayBounds() : bounds;
    }
  }
}
