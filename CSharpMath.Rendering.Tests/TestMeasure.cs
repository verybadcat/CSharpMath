namespace CSharpMath.Rendering.Tests {
  using System;
  using System.Drawing;
  using System.Linq;
  using BackEnd;
  using CSharpMath.Display.FrontEnd;
  using Xunit;

  public class TestMeasure {
    class D : Display.IDisplay<Fonts, Glyph> {
      public float Ascent => 12;
      public float Descent => 3;
      public float Width => 10;

      public PointF Position { get => PointF.Empty; set => throw new NotImplementedException(); }
      public Atom.Range Range => throw new NotImplementedException();
      public Color? TextColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Color? BackColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public bool HasScript { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public void Draw(IGraphicsContext<Fonts, Glyph> context) => throw new NotImplementedException();
      public void SetTextColorRecursive(Color? textColor) => throw new NotImplementedException();
    }
    class DKeyboard : Editor.MathKeyboard<Fonts, Glyph> {
      public DKeyboard() : base(TypesettingContext.Instance, new Fonts(Enumerable.Empty<Typography.OpenFont.Typeface>(), 0.0f)) =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
    }
    class DRenderingMath : SkiaSharp.MathPainter {
      public DRenderingMath() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
      protected override void UpdateDisplayCore(float unused) { }
    }
    class DRenderingText : SkiaSharp.TextPainter {
      public DRenderingText() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
      protected override void UpdateDisplayCore(float canvasWidth) { }
    }
    class DRenderingKeyboard : FrontEnd.MathKeyboard {
      public DRenderingKeyboard() =>
        Display = new Display.Displays.ListDisplay<Fonts, Glyph>(new[] { new D() });
    }
    /// <summary>
    /// CSharpMath uses the mathematical coordinate system,
    /// i.e. the rectangle position is at the bottom-left.
    /// </summary>
    [Fact]
    public void CoreMeasure_YIsNegDescent() {
      Assert.Equal(new RectangleF(0, -3, 10, 15), new D().DisplayBounds());
      Assert.Equal(new RectangleF(0, -3, 10, 15), new DKeyboard().Measure);
    }
    /// <summary>
    /// CSharpMath.Rendering and descendants use the graphical coordinate system,
    /// i.e. the rectangle position is at the top-left.
    /// </summary>
    [Fact]
    public void RenderingMeasure_YIsNegAscent() {
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingMath().Measure());
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingText().Measure(float.NaN));
      Assert.Equal(new RectangleF(0, -12, 10, 15), new DRenderingKeyboard().Measure);
    }

    [Fact]
    public void RenderingMeasure_IncludesTrailingGlyphInk() {
      var painter = new SkiaSharp.MathPainter { LaTeX = "V" };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      Assert.True(display.InkWidth() > display.Width);
      Assert.True(painter.Measure().Width >= display.InkWidth());
    }

    [Fact]
    public void AlignmentUsesInkExtentWithoutChangingAdvance() {
      var painter = new SkiaSharp.MathPainter { LaTeX = "V" };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      var centered = FrontEnd.IPainterExtensions.GetDisplayPosition(
        display.InkWidth(), display.Ascent, display.Descent, painter.FontSize,
        100, 100, FrontEnd.TextAlignment.Center, default, 0, 0);
      var expected = (100 - display.InkWidth()) / 2;
      Assert.Equal(expected, centered.X, precision: 4);
      Assert.True(display.Width < display.InkWidth());
    }

    [Theory]
    [InlineData(@"\frac{P}{2}")]
    [InlineData(@"\sqrt{P}")]
    [InlineData(@"\hat{P}")]
    [InlineData(@"\sum_{P}^{2}")]
    [InlineData(@"\overset{P}{2}")]
    [InlineData(@"\left(P\right)")]
    [InlineData(@"\text{P}")]
    public void CompositeInkIsRecursivelyAtLeastAdvance(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      var advance = display.Width;
      Assert.True(display.InkWidth() >= advance);
      Assert.Equal(advance, display.Width);
    }

    [Fact]
    public void StackDisplayClassControlsInterElementSpacing() {
      static (float Left, float Right) Gaps(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var stackIndex = root.Displays
          .Select((display, index) => (display, index))
          .Single(item => item.display is Display.Displays.StackDisplay<Fonts, Glyph>).index;
        var stack = root.Displays[stackIndex];
        var left = root.Displays[stackIndex - 1];
        var right = root.Displays[stackIndex + 1];
        return (
          stack.Position.X - (left.Position.X + left.Width),
          right.Position.X - (stack.Position.X + stack.Width));
      }

      var ordinary = Gaps(@"a\overset{x}{c}b");
      var binary = Gaps(@"a\stackbin{x}{+}b");
      var relation = Gaps(@"a\stackrel{x}{=}b");
      Assert.True(ordinary.Left < binary.Left);
      Assert.True(binary.Left < relation.Left);
      Assert.True(ordinary.Right < binary.Right);
      Assert.True(binary.Right < relation.Right);
    }

    [Theory]
    [InlineData(@"a+\stackrel{x}{=}b")]
    [InlineData(@"a=\stackbin{x}{+}b")]
    [InlineData(@"\stackbin{x}{+}b")]
    [InlineData(@"a+\bigm|b")]
    [InlineData(@"\bigm|+b")]
    [InlineData(@"\bigl(+b")]
    public void StackDisplayClassParticipatesInBinaryNormalization(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      Assert.Null(Record.Exception(() => painter.Measure()));
    }

    [Fact]
    public void MovingStackDisplayMovesItsChildren() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"a\overset{x}{c}b" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var stack = Assert.IsType<Display.Displays.StackDisplay<Fonts, Glyph>>(
        root.Displays.Single(display => display is Display.Displays.StackDisplay<Fonts, Glyph>));
      Assert.True(stack.Position.X > 0);
      Assert.True(stack.Base.Position.X >= stack.Position.X);
      Assert.NotNull(stack.Over);
      Assert.True(stack.Over.Position.X >= stack.Position.X);

      var oldPosition = stack.Position;
      var oldBasePosition = stack.Base.Position;
      var oldOverPosition = stack.Over.Position;
      stack.Position = new PointF(oldPosition.X + 7, oldPosition.Y + 3);
      Assert.Equal(new PointF(oldBasePosition.X + 7, oldBasePosition.Y + 3), stack.Base.Position);
      Assert.Equal(new PointF(oldOverPosition.X + 7, oldOverPosition.Y + 3), stack.Over.Position);
    }

    [Fact]
    public void GroupScriptsUseTheWholeGroupMetrics() {
      static float SuperscriptY(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        return root.Displays
          .OfType<Display.Displays.ListDisplay<Fonts, Glyph>>()
          .Single(display => display.LinePosition == Display.LinePosition.Superscript)
          .Position.Y;
      }

      Assert.True(SuperscriptY(@"{\frac{1}{2}+x}^3") > SuperscriptY(@"{x+y}^3"));
    }

    [Fact]
    public void ExtensibleStacksUseOpenTypeMinimumGaps() {
      static Display.Displays.StackDisplay<Fonts, Glyph> Stack(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        return Assert.IsType<Display.Displays.StackDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      }

      var font = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), FrontEnd.PainterConstants.DefaultFontSize);
      var over = Stack(@"\overrightarrow{x}");
      Assert.NotNull(over.Over);
      var aboveGap = over.Over.Position.Y - over.Over.Descent
        - (over.Base.Position.Y + over.Base.Ascent);
      Assert.Equal(MathTable.Instance.StretchStackGapAboveMin(font), aboveGap, precision: 4);

      var under = Stack(@"\underrightarrow{x}");
      Assert.NotNull(under.Under);
      var belowGap = under.Base.Position.Y - under.Base.Descent
        - (under.Under.Position.Y + under.Under.Ascent);
      Assert.Equal(MathTable.Instance.StretchStackGapBelowMin(font), belowGap, precision: 4);
    }

    [Fact]
    public void WideStackUsesTheCapGlyphHorizontalAssembly() {
      var painter = new SkiaSharp.MathPainter {
        LaTeX = @"\overrightarrow{ABCDEFGHIJKLMNOPQRSTUVWXYZ}"
      };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var stack = Assert.IsType<Display.Displays.StackDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      var over = Assert.IsType<Display.Displays.HorizontalGlyphConstructionDisplay<Fonts, Glyph>>(
        stack.Over);
      Assert.True(over.Width >= stack.Base.Width);
      Assert.True(over.Ascent > 0 || over.Descent > 0);
      Assert.Equal(stack.Range, over.Range);
    }

    [Fact]
    public void StackMathRowsUseRoleAppropriateCrampedness() {
      static float ScriptShift(string latex, bool over) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var stack = Assert.IsType<Display.Displays.StackDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
        var row = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(
          over ? stack.Over : stack.Under);
        var script = row.Displays
          .OfType<Display.Displays.ListDisplay<Fonts, Glyph>>()
          .Single(display => display.LinePosition == Display.LinePosition.Superscript);
        return script.Position.Y;
      }

      var underShift = ScriptShift(@"\underset{x^2}{y}", false);
      var overShift = ScriptShift(@"\overset{x^2}{y}", true);
      Assert.True(overShift > underShift);
    }

    [Fact]
    public void ContinuedFractionAppliesStrutFloorsToBothOperands() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"\cfrac{a}{b}" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var wrapper = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      var fraction = Assert.IsType<Display.Displays.FractionDisplay<Fonts, Glyph>>(
        Assert.Single(wrapper.Displays));

      Assert.True(fraction.Numerator.Ascent >= 0.85f * painter.FontSize - 0.001f);
      Assert.True(fraction.Numerator.Descent >= 0.35f * painter.FontSize - 0.001f);
      Assert.True(fraction.Denominator.Ascent >= 0.85f * painter.FontSize - 0.001f);
      Assert.True(fraction.Denominator.Descent >= 0.35f * painter.FontSize - 0.001f);
    }

    [Fact]
    public void ContinuedFractionUsesNormalDisplayFractionOperandStyle() {
      static Display.Displays.RadicalDisplay<Fonts, Glyph> DenominatorRadical(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var top = Assert.Single(root.Displays);
        var fraction = top is Display.Displays.FractionDisplay<Fonts, Glyph> direct
          ? direct
          : Assert.IsType<Display.Displays.FractionDisplay<Fonts, Glyph>>(
            Assert.Single(Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(top).Displays));
        return Assert.IsType<Display.Displays.RadicalDisplay<Fonts, Glyph>>(
          Assert.Single(fraction.Denominator.Displays));
      }

      var continued = DenominatorRadical(@"\cfrac{1}{\sqrt{\sqrt5}}");
      var display = DenominatorRadical(@"\dfrac{1}{\sqrt{\sqrt5}}");
      Assert.Equal(display.Width, continued.Width, precision: 4);
      Assert.Equal(display.TopKern, continued.TopKern, precision: 4);
      Assert.Equal(display.LineThickness, continued.LineThickness, precision: 4);
    }

    [Theory]
    [InlineData(@"\llap{P}")]
    [InlineData(@"\clap{P}")]
    public void LapBoxInkIsAvailableBeforeDraw(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      var advance = display.Width;
      Assert.True(display.InkWidth() > 0);
      Assert.Equal(advance, display.Width); // advance remains the box metric
    }

    [Fact]
    public void TrailingSuperscriptInkDoesNotChangeAdvance() {
      var painter = new SkiaSharp.MathPainter { LaTeX = "P^P" };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      var advance = display.Width;
      Assert.True(display.InkWidth() >= advance);
      Assert.Equal(advance, display.Width);
    }

    [Fact]
    public void OrdinaryItalicCorrectionIsAppliedBeforeCloseAtom() {
      static Display.Displays.TextLineDisplay<Fonts, Glyph> Line(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        return Assert.IsType<Display.Displays.TextLineDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      }

      var line = Line(@"P)");
      var run = Assert.Single(line.Runs).Run;
      Assert.Equal(2, run.GlyphInfos.Count);
      Assert.NotEqual(0, run.GlyphInfos[0].KernAfterGlyph);
      Assert.Equal(0, run.GlyphInfos[1].KernAfterGlyph);
    }

    [Fact]
    public void OrdinaryItalicCorrectionSurvivesStyledRunBoundary() {
      static Display.Displays.TextLineDisplay<Fonts, Glyph> Line(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        return Assert.IsType<Display.Displays.TextLineDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      }

      var run = Assert.Single(Line(@"\mathit{P}\mathrm{Q})").Runs).Run;
      var withoutClose = Assert.Single(Line(@"\mathit{P}\mathrm{Q}").Runs).Run;
      Assert.Equal(3, run.GlyphInfos.Count);
      Assert.Equal(2, withoutClose.GlyphInfos.Count);
      Assert.Equal(withoutClose.GlyphInfos[0].KernAfterGlyph, run.GlyphInfos[0].KernAfterGlyph);
      Assert.Equal(0, run.GlyphInfos[2].KernAfterGlyph);
    }

    [Fact]
    public void UprightFusedRunSuppressesInteriorCorrection() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"\mathrm{PQ}" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var line = Assert.IsType<Display.Displays.TextLineDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      var glyphs = Assert.Single(line.Runs).Run.GlyphInfos;
      Assert.Equal(2, glyphs.Count);
      Assert.Equal(0, glyphs[0].KernAfterGlyph);
    }

    [Fact]
    public void OrdinaryItalicCorrectionAccumulatesWithBinarySpacing() {
      static Display.Displays.TextLineDisplay<Fonts, Glyph> Line(string latex) {
        var painter = new SkiaSharp.MathPainter { LaTeX = latex };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        return Assert.IsType<Display.Displays.TextLineDisplay<Fonts, Glyph>>(Assert.Single(root.Displays));
      }

      var close = Line(@"P)").Runs.Single().Run.GlyphInfos[0].KernAfterGlyph;
      var binary = Line(@"P+Q").Runs.Single().Run.GlyphInfos[0].KernAfterGlyph;
      var font = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), FrontEnd.PainterConstants.DefaultFontSize);
      var binarySpacing = 4 * MathTable.Instance.MuUnit(font);
      Assert.Equal(close + binarySpacing, binary, precision: 4);
    }

    [Fact]
    public void ScriptedFinalGlyphIsCorrectedOnlyByScriptLayout() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"P^2" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var line = Assert.IsType<Display.Displays.TextLineDisplay<Fonts, Glyph>>(
        root.Displays.Single(display => display is Display.Displays.TextLineDisplay<Fonts, Glyph>));
      Assert.Equal(0, Assert.Single(line.Runs).Run.GlyphInfos[0].KernAfterGlyph);
      Assert.Single(root.Displays.OfType<Display.Displays.ListDisplay<Fonts, Glyph>>(),
        display => display.LinePosition == Display.LinePosition.Superscript);
    }

    [Fact]
    public void LargeOperatorItalicCorrectionIsIsolatedFromFollowingRun() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"\sum P" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      Assert.Contains(root.Displays, display => display is Display.Displays.GlyphDisplay<Fonts, Glyph>);
      var line = Assert.Single(root.Displays.OfType<Display.Displays.TextLineDisplay<Fonts, Glyph>>());
      Assert.Single(line.Runs);
      Assert.Single(line.Runs[0].Run.GlyphInfos);
    }

    [Theory]
    [InlineData(@"P\sum")]
    public void ItalicCorrectionSurvivesCompositeFollowingAtom(string latex) {
      static float FirstKern(string source) {
        var painter = new SkiaSharp.MathPainter { LaTeX = source };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var line = Assert.Single(root.Displays.OfType<Display.Displays.TextLineDisplay<Fonts, Glyph>>());
        return Assert.Single(line.Runs).Run.GlyphInfos[0].KernAfterGlyph;
      }

      Assert.Equal(FirstKern(@"P)"), FirstKern(latex), precision: 4);
    }

    [Theory]
    [InlineData(@"P\quad")]
    [InlineData(@"P\displaystyle")]
    public void NonDisplayingTerminalAtomDoesNotApplyItalicCorrection(string latex) {
      static float Width(string source) {
        var painter = new SkiaSharp.MathPainter { LaTeX = source };
        painter.Measure();
        return painter.Display!.Width;
      }

      Assert.Equal(Width("P"), Width(latex), precision: 4);
    }

    [Theory]
    [InlineData(@"P\displaystyle )")]
    public void NonDisplayingAtomPreservesPendingItalicCorrection(string latex) {
      static float FirstKern(string source) {
        var painter = new SkiaSharp.MathPainter { LaTeX = source };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var line = Assert.Single(root.Displays.OfType<Display.Displays.TextLineDisplay<Fonts, Glyph>>());
        return Assert.Single(line.Runs).Run.GlyphInfos[0].KernAfterGlyph;
      }
      static float Gap(string source) {
        var painter = new SkiaSharp.MathPainter { LaTeX = source };
        painter.Measure();
        var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
        var lines = root.Displays.OfType<Display.Displays.TextLineDisplay<Fonts, Glyph>>().ToArray();
        Assert.Equal(2, lines.Length);
        return lines[1].Position.X - (lines[0].Position.X + lines[0].Width);
      }

      var correction = FirstKern(@"P)");
      if (latex.Contains(@"\quad"))
        Assert.Equal(Gap(@"Q\quad Q") + correction, Gap(latex), precision: 4);
      else
        Assert.Equal(correction, Gap(latex), precision: 4);
    }

    [Theory]
    [InlineData(FrontEnd.TextAlignment.Center)]
    [InlineData(FrontEnd.TextAlignment.Right)]
    public void FiniteCanvasAlignmentUsesInkWidth(FrontEnd.TextAlignment alignment) {
      var painter = new SkiaSharp.MathPainter { LaTeX = "P" };
      painter.Measure();
      var display = painter.Display;
      Assert.NotNull(display);
      var x = FrontEnd.IPainterExtensions.GetDisplayPosition(
        display.InkWidth(), display.Ascent, display.Descent, painter.FontSize,
        100, 100, alignment, default, 0, 0).X;
      var expected = alignment == FrontEnd.TextAlignment.Right
        ? 100 - display.InkWidth()
        : (100 - display.InkWidth()) / 2;
      Assert.Equal(expected, x, precision: 4);
      Assert.True(display.Width < display.InkWidth());
    }

    static Display.Displays.TextLineDisplay<Fonts, Glyph> FirstLine(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      static Display.Displays.TextLineDisplay<Fonts, Glyph>? Find(Display.IDisplay<Fonts, Glyph> display) {
        if (display is Display.Displays.TextLineDisplay<Fonts, Glyph> line) return line;
        if (display is Display.Displays.ListDisplay<Fonts, Glyph> list)
          return list.Displays.Select(Find).FirstOrDefault(line => line != null);
        if (display is Display.Displays.BoxDisplay<Fonts, Glyph> box)
          return Find(box.Child);
        if (display is Display.Displays.InnerDisplay<Fonts, Glyph> inner)
          return Find(inner.Inner);
        return null;
      }

      return Find(painter.Display!)!;
    }

    static float FirstKern(string latex) =>
      Assert.Single(FirstLine(latex).Runs).Run.GlyphInfos[0].KernAfterGlyph;

    [Theory]
    [InlineData(@"P)")]
    [InlineData(@"P2")]
    [InlineData("PΑ")]
    public void OrdinaryItalicCorrectionHandlesStraightUnicodeSuccessors(string latex) {
      Assert.Equal(FirstKern(@"P)"), FirstKern(latex), precision: 4);
      Assert.NotEqual(0, FirstKern(latex));
    }

    [Theory]
    [InlineData(@"Ph")]
    [InlineData("P\U0001D452")] // mathematical italic small e
    [InlineData("P\U0001D468")] // mathematical bold italic capital A
    public void OrdinaryItalicCorrectionDoesNotSeparateSlantedUnicodeSuccessors(string latex) {
      Assert.Equal(0, FirstKern(latex));
    }

    [Fact]
    public void FusedOrdinaryRunAppliesEveryInternalItalicCorrection() {
      var glyphs = Assert.Single(FirstLine("P2Q3").Runs).Run.GlyphInfos;

      Assert.Equal(4, glyphs.Count);
      Assert.Equal(FirstKern("P)"), glyphs[0].KernAfterGlyph, precision: 4);
      Assert.Equal(0, glyphs[1].KernAfterGlyph);
      Assert.Equal(FirstKern("Q)"), glyphs[2].KernAfterGlyph, precision: 4);
      Assert.Equal(0, glyphs[3].KernAfterGlyph);
    }

    [Fact]
    public void FusedInternalItalicCorrectionSurvivesScriptOnStraightSuccessor() {
      var glyphs = Assert.Single(FirstLine("P2^3").Runs).Run.GlyphInfos;

      Assert.Equal(2, glyphs.Count);
      Assert.Equal(FirstKern("P)"), glyphs[0].KernAfterGlyph, precision: 4);
      Assert.Equal(0, glyphs[1].KernAfterGlyph);
    }

    [Fact]
    public void OrdinaryItalicCorrectionIsAddedToExistingRule16Spacing() {
      var close = FirstKern(@"P)");
      var binary = FirstKern(@"P+Q");
      var font = new Fonts(Array.Empty<Typography.OpenFont.Typeface>(), FrontEnd.PainterConstants.DefaultFontSize);
      Assert.Equal(close + 4 * MathTable.Instance.MuUnit(font), binary, precision: 4);
    }

    [Theory]
    [InlineData(@"P^2)")]
    [InlineData(@"P_2)")]
    [InlineData(@"{P Q}^2)")]
    [InlineData(@"\boxed{P}2")]
    [InlineData(@"P\frac{2}{3}")]
    [InlineData(@"P\begin{array}{c}2\\3\end{array}")]
    public void ScriptAndCompositePredecessorsDoNotDuplicateItalicCorrection(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      Assert.Null(Record.Exception(() => painter.Measure()));
      var line = FirstLine(latex);
      Assert.All(line.Runs.SelectMany(run => run.Run.GlyphInfos), glyph =>
        Assert.InRange(glyph.KernAfterGlyph, 0, FirstKern(@"P)")));
    }

    [Theory]
    [InlineData(@"\color{red}{P})")]
    [InlineData(@"{P})")]
    public void WrapperBoundariesRetainSingleTrailingItalicCorrection(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      Assert.Equal(MeasureWidth(@"P)"), painter.Display!.Width, precision: 4);
    }

    [Fact]
    public void ColorBoxBoundaryIncludesItsPaddingOnce() {
      var painter = new SkiaSharp.MathPainter { LaTeX = @"\colorbox{red}{P})" };
      painter.Measure();
      Assert.Equal(MeasureWidth(@"P)"), painter.Display!.Width, precision: 4);
      Assert.True(painter.Display.InkWidth() >= painter.Display.Width);
    }

    [Theory]
    [InlineData(@"\left.P\right)")]
    [InlineData(@"\left(P\right)")]
    public void InnerBoundariesIncludeTheRightDelimiterAfterTheNestedContent(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      Assert.True(painter.Display!.Width > MeasureWidth("P"));
      Assert.True(painter.Display.InkWidth() >= painter.Display.Width);
    }

    static float MeasureWidth(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      return painter.Measure().Width;
    }

    [Fact]
    public void ItalicCorrectionPreservesPrecedingAdvanceAndNestedMatrixPlacement() {
      var simple = new SkiaSharp.MathPainter { LaTeX = @"QP)" };
      var matrix = new SkiaSharp.MathPainter { LaTeX = @"\begin{pmatrix}P)\\Q\end{pmatrix}" };
      simple.Measure();
      matrix.Measure();
      Assert.True(simple.Display!.Width > new SkiaSharp.MathPainter { LaTeX = @"P)" }.Measure().Width);
      var matrixDisplay = matrix.Display!;
      Assert.True(matrixDisplay.InkWidth() >= matrixDisplay.Width);
      Assert.True(matrixDisplay.Width > 0);
    }

    [Theory]
    [InlineData(@"P")]
    [InlineData(@"\left.P\right)")]
    [InlineData(@"\begin{pmatrix}P&Q\\R&S\end{pmatrix}")]
    public void TerminalInkExtentNeverShrinksTheAdvance(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      painter.Measure();
      Assert.NotNull(painter.Display);
      Assert.True(painter.Display!.InkWidth() >= painter.Display.Width);
    }
  }
}
