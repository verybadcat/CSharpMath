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
  }
}
