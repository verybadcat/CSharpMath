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
