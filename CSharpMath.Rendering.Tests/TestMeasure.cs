namespace CSharpMath.Rendering.Tests {
  using System;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using BackEnd;
  using CSharpMath.Display.FrontEnd;
  using SkiaSharp;
  using Typography.OpenFont.Extensions;
  using Xunit;

  [Collection(nameof(TestRenderingFixture))]
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
    public void ReaderLoadedCffFont_AddOverridePreservesMeasureAndInkBounds() {
      const string formula = "Sample text $$x+3$$";
      var baseline = ReadMathFont();
      baseline!.UpdateAllCffGlyphBounds();
      var baselinePainter = new SkiaSharp.MathPainter {
        FontSize = 48, LocalTypefaces = new[] { baseline! }, LaTeX = formula
      };
      var baselineMeasure = baselinePainter.Measure(float.PositiveInfinity);
      using var baselineImage = baselinePainter.DrawAsStream(float.PositiveInfinity);

      // Exercise the public CSharpMath ingestion path, rather than relying on
      // the reader or a global test-side bounds update.
      var candidate = ReadMathFont();
      var candidateTypefaces = new Typefaces(baseline!);
      candidateTypefaces.AddOverride(candidate!);
      var candidatePainter = new SkiaSharp.MathPainter {
        FontSize = 48, LocalTypefaces = candidateTypefaces, LaTeX = formula
      };
      var candidateMeasure = candidatePainter.Measure(float.PositiveInfinity);
      using var candidateImage = candidatePainter.DrawAsStream(float.PositiveInfinity);

      Assert.Equal(baselineMeasure.Width, candidateMeasure.Width, 3);
      Assert.Equal(baselineMeasure.Height, candidateMeasure.Height, 3);
      Assert.Equal(ReadBytes(baselineImage!), ReadBytes(candidateImage!));
      Assert.True(candidateMeasure.Height > 40);
    }

    [Fact]
    public void ReaderLoadedCffFont_AddOverrideSupportsMathStretching() {
      var baseline = ReadMathFont();
      baseline!.UpdateAllCffGlyphBounds();
      var candidate = ReadMathFont();
      var baselineTypefaces = new Typefaces(baseline!);
      var candidateTypefaces = new Typefaces(baseline!);
      candidateTypefaces.AddOverride(candidate!);

      var baselinePainter = new SkiaSharp.MathPainter {
        FontSize = 48,
        LocalTypefaces = baselineTypefaces,
        LaTeX = "$$\\sqrt{\\frac{x^2+1}{y}} + \\sum_{i=1}^{8} x_i$$"
      };
      var candidatePainter = new SkiaSharp.MathPainter {
        FontSize = 48,
        LocalTypefaces = candidateTypefaces,
        LaTeX = "$$\\sqrt{\\frac{x^2+1}{y}} + \\sum_{i=1}^{8} x_i$$"
      };
      var baselineMeasure = baselinePainter.Measure(float.PositiveInfinity);
      var candidateMeasure = candidatePainter.Measure(float.PositiveInfinity);
      Assert.Equal(baselineMeasure.Width, candidateMeasure.Width, 3);
      Assert.Equal(baselineMeasure.Height, candidateMeasure.Height, 3);
      Assert.True(candidateMeasure.Height > 100);
    }

    static Typography.OpenFont.Typeface? ReadMathFont() {
      using var stream = typeof(Fonts).Assembly.GetManifestResourceStream(
        "CSharpMath.Rendering.Reference_Fonts.latinmodern-math.otf");
      Assert.NotNull(stream);
      var typeface = new Typography.OpenFont.OpenFontReader().Read(stream!);
      Assert.NotNull(typeface);
      Assert.True(typeface!.IsCffFont);
      return typeface;
    }

    static byte[] ReadBytes(Stream imageStream) {
      imageStream.Position = 0;
      using var copy = new MemoryStream();
      imageStream.CopyTo(copy);
      return copy.ToArray();
    }
  }
}
