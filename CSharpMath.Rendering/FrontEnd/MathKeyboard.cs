namespace CSharpMath.Rendering.FrontEnd {
  using System;
  using System.Drawing;
  using BackEnd;
  using Display;
  using Display.Displays;
  using Editor;

  public enum CaretShape { IBeam, UpArrow }
  public class MathKeyboard : MathKeyboard<Fonts, Glyph> {
    public MathKeyboard(float fontSize = PainterConstants.DefaultFontSize, double blinkMilliseconds = DefaultBlinkMilliseconds)
      : base(TypesettingContext.Instance,
             new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), fontSize), blinkMilliseconds) { }
    // Rendering: Convert to 
    public override RectangleF Measure =>
      Display != null ? new RectangleF(0, -Display.Ascent, Display.Width, Display.Ascent + Display.Descent) : RectangleF.Empty;
    public void DrawCaret(ICanvas canvas, Color color, CaretShape shape) {
      if (Display == null)
        return;
      var cursorPosition = Display.PointForIndex(TypesettingContext.Instance, InsertionIndex) ?? Display.Position;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      var caretDisplay = DisplayForCaret(Display, InsertionIndex) ?? Display;
      var caretAscent = caretDisplay.Ascent;
      var caretDescent = caretDisplay.Descent;
      // An empty list has no display metrics. Keep the caret visible in that
      // case using the same font-relative height as the original caret.
      if (caretAscent + caretDescent <= 0) {
        caretAscent = Font.PointSize * 2 / 3;
        caretDescent = 0;
      }
      using var path = canvas.StartNewPath();
      path.Foreground = color;
      path.MoveTo(cursorPosition.X, cursorPosition.Y);
      switch (shape) {
        case CaretShape.IBeam:
          // Use the display at the insertion point rather than the keyboard's
          // font size. This keeps the caret useful in scripts and nested lists.
          var overshoot = Math.Min(2, Math.Max(1, Font.PointSize / 16));
          ReadOnlySpan<PointF> s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 16, caretDescent + overshoot),
            new PointF(Font.PointSize / 2 / 16, -caretAscent - overshoot),
            new PointF(-Font.PointSize / 2 / 16, -caretAscent - overshoot),
            new PointF(-Font.PointSize / 2 / 16, caretDescent + overshoot),
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
        case CaretShape.UpArrow:
          s = stackalloc PointF[4] {
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4),
            new PointF(Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3),
            new PointF(-Font.PointSize / 2 / 2, Font.PointSize * 2 / 3 / 4)
          };
          foreach (var p in s)
            path.LineTo(p.X + cursorPosition.X, p.Y + cursorPosition.Y);
          break;
      }
      path.CloseContour();
    }

    private static IDisplay<Fonts, Glyph>? DisplayForCaret(
      IDisplay<Fonts, Glyph> display, MathListIndex index) {
      if (index is null)
        return null;
      if (display is ListDisplay<Fonts, Glyph> list) {
        if (index.AtomIndex == list.Range.End) {
          // A regular list's range ends after its last non-script child;
          // use that child rather than inheriting metrics from the aggregate
          // list (or from a trailing script).
          for (var i = list.Displays.Count - 1; i >= 0; i--)
            if (list.Displays[i] is not ListDisplay<Fonts, Glyph> { LinePosition: not LinePosition.Regular })
              return list.Displays[i];
          return list;
        }
        var child = list.SubDisplayForIndex(index);
        if (child is null)
          return list;
        return index.SubIndexInfo is null or (MathListSubIndexType.BetweenBaseAndScripts, _)
          ? child
          : DisplayForCaret(child, index.SubIndexInfo.Value.SubIndex);
      }
      if (index.SubIndexInfo is not { } info)
        return display;
      IDisplay<Fonts, Glyph>? childDisplay = display switch {
        FractionDisplay<Fonts, Glyph> fraction => info.SubIndexType switch {
          MathListSubIndexType.Numerator => fraction.Numerator,
          MathListSubIndexType.Denominator => fraction.Denominator,
          _ => null,
        },
        RadicalDisplay<Fonts, Glyph> radical => info.SubIndexType switch {
          MathListSubIndexType.Degree => radical.Degree,
          MathListSubIndexType.Radicand => radical.Radicand,
          _ => null,
        },
        InnerDisplay<Fonts, Glyph> inner when info.SubIndexType == MathListSubIndexType.Inner => inner.Inner,
        LargeOpLimitsDisplay<Fonts, Glyph> limits => limits.SubListForIndexType(info.SubIndexType),
        _ => null,
      };
      return childDisplay is null ? display : DisplayForCaret(childDisplay, info.SubIndex);
    }
  }
}
