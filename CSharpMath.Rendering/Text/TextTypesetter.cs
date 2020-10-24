namespace CSharpMath.Rendering.Text {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Atom;
  using Display;
  using Display.Displays;
  using BackEnd;
  using FrontEnd;
  using Display = Display.Displays.ListDisplay<BackEnd.Fonts, BackEnd.Glyph>;
  using CSharpMath.Structures;
  using System.Drawing;
  public static class TextTypesetter {
    public static (Display relative, Display absolute) Layout(TextAtom input, Fonts inputFont, float canvasWidth) {
      // TODO: Multiply these constants by resolution
      const float abovedisplayskip = 12, abovedisplayshortskip = 0,
                  belowdisplayskip = 12, belowdisplayshortskip = 7;
      if (input == null) return
          (new Display(Array.Empty<IDisplay<Fonts, Glyph>>()),
           new Display(Array.Empty<IDisplay<Fonts, Glyph>>()));
      float accumulatedHeight = 0;
      //indicator of the need to apply belowdisplay(short)skip when line break
      bool afterDisplayMaths = false;
      void BreakLine(TextLayoutLineBuilder line, List<IDisplay<Fonts, Glyph>> displayList,
        List<IDisplay<Fonts, Glyph>> displayMathList, bool appendLineGap = true) {
        if (afterDisplayMaths) {
          accumulatedHeight +=
            line.Width > displayMathList.Last().Position.X
            ? belowdisplayskip
            : belowdisplayshortskip;
          afterDisplayMaths = false;
        }
        line.Clear(0, -accumulatedHeight, displayList,
          ref accumulatedHeight, true, appendLineGap);
      }
      //variables captured by this method are currently unchangable by TextAtoms
      void AddDisplaysWithLineBreaks(
        TextAtom atom,
        Fonts fonts,
        TextLayoutLineBuilder line,
        List<IDisplay<Fonts, Glyph>> displayList,
        List<IDisplay<Fonts, Glyph>> displayMathList,
        FontStyle style,
        Color? color
      ) {

        IDisplay<Fonts, Glyph> display;
        switch (atom) {
          case TextAtom.List list:
            foreach (var a in list.Content)
              AddDisplaysWithLineBreaks
                (a, fonts, line, displayList, displayMathList, style, color);
            break;
          case TextAtom.Style st:
            AddDisplaysWithLineBreaks
              (st.Content, fonts, line, displayList, displayMathList, st.FontStyle, color);
            break;
          case TextAtom.Size sz:
            AddDisplaysWithLineBreaks
              (sz.Content, new Fonts(fonts, sz.PointSize), line, displayList,
               displayMathList, style, color);
            break;
          case TextAtom.Colored c:
            AddDisplaysWithLineBreaks
              (c.Content, fonts, line, displayList, displayMathList, style, c.Colour);
            break;
          case TextAtom.Space sp:
            //Allow space at start of line since user explicitly specified its length
            //Also \par generates this kind of spaces
            line.AddSpace(sp.Content.ActualLength(MathTable.Instance, fonts));
            break;
          case TextAtom.Newline n:
            BreakLine(line, displayList, displayMathList);
            break;
          case TextAtom.Math m when m.DisplayStyle:
            var lastLineWidth = line.Width;
            BreakLine(line, displayList, displayMathList, false);
            display = Typesetter.CreateLine(m.Content, fonts, TypesettingContext.Instance, LineStyle.Display);
            var displayX = IPainterExtensions.GetDisplayPosition
              (display.Width, display.Ascent, display.Descent,
               fonts.PointSize, canvasWidth, float.NaN,
               TextAlignment.Top, default, default, default).X;
            //\because When displayList.LastOrDefault() is null,
            //the false condition is selected
            //\therefore Append abovedisplayshortskip which defaults
            //to 0 when nothing is above the display-style maths
            accumulatedHeight +=
              lastLineWidth > displayX ? abovedisplayskip : abovedisplayshortskip;
            accumulatedHeight += display.Ascent;
            display.Position = new System.Drawing.PointF(displayX, -accumulatedHeight);
            accumulatedHeight += display.Descent;
            afterDisplayMaths = true;
            if (color != null) display.SetTextColorRecursive(color);
            displayMathList.Add(display);
            break;

            void FinalizeInlineDisplay(float ascender, float rawDescender,
                float lineGap, bool forbidAtLineStart = false) {
              if (color != null) display.SetTextColorRecursive(color);
              if (line.Width + display.Width > canvasWidth && !forbidAtLineStart)
                BreakLine(line, displayList, displayMathList);
              //rawDescender is taken directly from font file and is negative,
              //while IDisplay.Descender is positive
              line.Add(display, ascender, -rawDescender, lineGap);
            }
          case TextAtom.Text t:
            var content = UnicodeFontChanger.ChangeFont(t.Content, style);
            var glyphs = GlyphFinder.Instance.FindGlyphs(fonts, content);
            //Calling Select(g => g.Typeface).Distinct() speeds up query up to 10 times,
            //Calling Max(Func<,>) instead of Select(Func<,>).Max() speeds up query 2 times
            var typefaces = glyphs.Select(g => g.Typeface).Distinct().ToList();
            display = new TextRunDisplay<Fonts, Glyph>(
              new AttributedGlyphRun<Fonts, Glyph>(content, glyphs, fonts),
              Range.NotFound, TypesettingContext.Instance
            );
            FinalizeInlineDisplay(
              typefaces.Max(tf => tf.Ascender * tf.CalculateScaleToPixelFromPointSize(fonts.PointSize)),
              typefaces.Min(tf => tf.Descender * tf.CalculateScaleToPixelFromPointSize(fonts.PointSize)),
              typefaces.Max(tf => tf.LineGap * tf.CalculateScaleToPixelFromPointSize(fonts.PointSize))
            );
            break;
          case TextAtom.Math m:
            if (m.DisplayStyle)
              throw new InvalidCodePathException
                ("Display style maths should have been handled above this switch.");
            display = Typesetter.CreateLine(m.Content, fonts, TypesettingContext.Instance, LineStyle.Text);
            var scale = fonts.MathTypeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
            FinalizeInlineDisplay(fonts.MathTypeface.Ascender * scale,
              fonts.MathTypeface.Descender * scale, fonts.MathTypeface.LineGap * scale);
            break;
          case TextAtom.ControlSpace cs:
            var spaceGlyph = GlyphFinder.Instance.Lookup(fonts, ' ');
            display = new TextRunDisplay<Fonts, Glyph>(
              new AttributedGlyphRun<Fonts, Glyph>(" ", new[] { spaceGlyph }, fonts),
              Range.NotFound, TypesettingContext.Instance
            );
            scale = spaceGlyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
            FinalizeInlineDisplay(spaceGlyph.Typeface.Ascender * scale,
              spaceGlyph.Typeface.Descender * scale,
              spaceGlyph.Typeface.LineGap * scale,
              forbidAtLineStart: true); //No spaces at start of line
            break;
          case TextAtom.Accent a:
            var accentGlyph = GlyphFinder.Instance.FindGlyphForCharacterAtIndex(
              fonts, a.AccentChar.Length - 1, a.AccentChar
            );
            scale = accentGlyph.Typeface.CalculateScaleToPixelFromPointSize(fonts.PointSize);
            var accenteeDisplayList = new List<IDisplay<Fonts, Glyph>>();
            var invalidDisplayMaths = new List<IDisplay<Fonts, Glyph>>();
            var accentDisplayLine = new TextLayoutLineBuilder();
            AddDisplaysWithLineBreaks(a.Content, fonts, accentDisplayLine,
              accenteeDisplayList, invalidDisplayMaths, style, color);
            float _ = default;
            accentDisplayLine.Clear
              (0, 0, accenteeDisplayList, ref _, false, false);
            System.Diagnostics.Debug.Assert(invalidDisplayMaths.Count == 0,
              "Display maths inside an accentee is unsupported -- ignoring display maths");
            var accentee = new Display(accenteeDisplayList);
            var accenteeCodepoint = a.Content.SingleChar(style);
            var accenteeSingleGlyph =
              accenteeCodepoint.HasValue
              ? GlyphFinder.Instance.Lookup(fonts, accenteeCodepoint.GetValueOrDefault())
              : GlyphFinder.Instance.EmptyGlyph;

            var accentDisplay = new AccentDisplay<Fonts, Glyph>(
              Typesetter.CreateAccentGlyphDisplay(
                accentee, accenteeSingleGlyph, accentGlyph,
                TypesettingContext.Instance, fonts, Range.NotFound
              ), accentee);
            display = accentDisplay;
            //accentDisplay.Ascent does not take account of accent glyph's extra height
            //-> accent will be out of bounds if it is on the first line
            FinalizeInlineDisplay(
              Math.Max(accentGlyph.Typeface.Ascender * scale,
                accentDisplay.Accent.Position.Y + accentDisplay.Ascent),
              accentGlyph.Typeface.Descender * scale,
              accentGlyph.Typeface.LineGap * scale);
            break;
          case TextAtom.Comment _:
            break;
          case null:
            throw new InvalidOperationException
              ("TextAtoms should never be null. You must have sneaked one in.");
          case var a:
            throw new InvalidCodePathException
              ($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
        }
      }
      var relativePositionList = new List<IDisplay<Fonts, Glyph>>();
      var absolutePositionList = new List<IDisplay<Fonts, Glyph>>();
      var globalLine = new TextLayoutLineBuilder();
      AddDisplaysWithLineBreaks(
        input,
        inputFont,
        globalLine,
        relativePositionList,
        absolutePositionList,
        FontStyle.Roman /*FontStyle.Default is FontStyle.Italic, FontStyle.Roman is no change to characters*/,
        null
      );
      BreakLine(globalLine, relativePositionList, absolutePositionList); //remember to finalize the last line
      var adjustedCanvasWidth =
        float.IsInfinity(canvasWidth) || float.IsNaN(canvasWidth)
        ? Math.Max(relativePositionList.CollectionWidth(),
                   absolutePositionList.IsNonEmpty() ? absolutePositionList.Max(d => d.Width) : 0)
        : canvasWidth;
      if (float.IsInfinity(canvasWidth) || float.IsNaN(canvasWidth))
        // In this case X of every display in absolutePositionList will be Infinity or NaN
        // Use max(width of relativePositionList, width of absolutePositionList) as canvasWidth instead
        foreach (var absDisplay in absolutePositionList)
          absDisplay.Position = new System.Drawing.PointF(
            IPainterExtensions.GetDisplayPosition
              (absDisplay.Width, absDisplay.Ascent, absDisplay.Descent,
               inputFont.PointSize, adjustedCanvasWidth, float.NaN,
               TextAlignment.Top, default, default, default).X,
            absDisplay.Position.Y);
      return (new Display(relativePositionList), new Display(absolutePositionList));
    }
  }
}