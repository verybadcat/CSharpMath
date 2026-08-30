using System;
using System.Drawing;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Rendering;
using CSharpMath.Rendering.BackEnd;
using CSharpMath.Rendering.FrontEnd;
using CSharpMath.SkiaSharp;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  public class TestMultilineBounds {
    const string Maxwell = @"\begin{aligned}\nabla \cdot \vec{\bf E} & = \frac {\rho} {\varepsilon_0} \\ \nabla \cdot \vec{\bf B} & = 0 \\ \nabla \times \vec{\bf E} &= - \frac{\partial\vec{\bf B}}{\partial t} \\ \nabla \times \vec{\bf B} & = \mu_0\vec{\bf J} + \mu_0\varepsilon_0 \frac{\partial\vec{\bf E}}{\partial t}\end{aligned}";
    const string EqnArrayMath = @"\begin{eqnarray}a+b+c+d+e+f&=&a+b+c+d+e+f\\0 &=& a+b+c+d+e+f-a-b-c-d-e-f\end{eqnarray}";
    const string EqnArrayText = @"$$\begin{eqnarray}a+b+c+d+e+f&=&a+b+c+d+e+f\\0 &=& a+b+c+d+e+f-a-b-c-d-e-f\end{eqnarray}$$";

    [Theory]
    [InlineData(Maxwell)]
    [InlineData(EqnArrayMath)]
    [InlineData(@"\begin{array}{cc}a&b\\c&d\end{array}")]
    [InlineData(@"\begin{matrix}1&2\\3&4\end{matrix}")]
    public void MultilineSourcesHaveFiniteAggregateMeasure(string source) {
      var painter = new SkiaSharp.MathPainter { FontSize = 32, LaTeX = source };
      var bounds = painter.Measure(600);
      Assert.Null(painter.ErrorMessage);
      Assert.True(bounds.Width > 0 && bounds.Height > 0);
      Assert.True(float.IsFinite(bounds.Left) && float.IsFinite(bounds.Top));
    }

    [Fact]
    public void MeasureDoesNotChangeLaterDrawLayoutWidth() {
      var afterMeasure = new SkiaSharp.TextPainter { FontSize = 32, LaTeX = "line one line two" };
      afterMeasure.Measure(100);
      using var first = new SKBitmap(1000, 1000);
      using var firstCanvas = new SKCanvas(first);
      afterMeasure.Draw(firstCanvas, TextAlignment.TopLeft);

      var direct = new SkiaSharp.TextPainter { FontSize = 32, LaTeX = "line one line two" };
      using var second = new SKBitmap(1000, 1000);
      using var secondCanvas = new SKCanvas(second);
      direct.Draw(secondCanvas, TextAlignment.TopLeft);
      Assert.Equal(direct._relativeXCoordDisplay.Position, afterMeasure._relativeXCoordDisplay.Position);
    }

    [Fact]
    public void DrawIsStatelessAcrossRepeatedCalls() {
      var painter = new SkiaSharp.TextPainter { FontSize = 32, LaTeX = "line one line two" };
      using var bitmap = new SKBitmap(1000, 1000);
      using var canvas = new SKCanvas(bitmap);
      painter.Draw(canvas, TextAlignment.TopLeft);
      var first = painter._relativeXCoordDisplay.Position;
      painter.Draw(canvas, TextAlignment.TopLeft);
      Assert.Equal(first, painter._relativeXCoordDisplay.Position);
    }

    [Theory]
    [InlineData(TextAlignment.TopLeft)]
    [InlineData(TextAlignment.Top)]
    public void DrawAsStreamDimensionsAndEdgesContainInk(TextAlignment alignment) {
      var painter = new SkiaSharp.MathPainter { FontSize = 32, LaTeX = Maxwell };
      using var stream = painter.DrawAsStream(600, alignmentForTests: alignment);
      using var bitmap = SKBitmap.Decode(stream);
      Assert.True(bitmap.Width > 0 && bitmap.Height > 0);
      var pixels = Enumerable.Range(0, bitmap.Width).SelectMany(x =>
        Enumerable.Range(0, bitmap.Height).Select(y => (x, y, a: bitmap.GetPixel(x, y).Alpha)))
        .Where(p => p.a > 0).ToArray();
      Assert.NotEmpty(pixels);
      Assert.InRange(pixels.Min(p => p.x), 1, bitmap.Width - 2);
      Assert.InRange(pixels.Max(p => p.x), 1, bitmap.Width - 2);
      Assert.InRange(pixels.Min(p => p.y), 1, bitmap.Height - 2);
      Assert.InRange(pixels.Max(p => p.y), 1, bitmap.Height - 2);
    }

    [Fact]
    public void EqnArrayStreamContainsTheCompleteComposite() {
      var painter = new SkiaSharp.TextPainter { FontSize = 32, LaTeX = EqnArrayText };
      using var stream = painter.DrawAsStream(600);
      using var bitmap = SKBitmap.Decode(stream);
      var pixels = Enumerable.Range(0, bitmap.Width).SelectMany(x =>
        Enumerable.Range(0, bitmap.Height).Select(y => (x, y, a: bitmap.GetPixel(x, y).Alpha)))
        .Where(p => p.a > 0).ToArray();
      Assert.NotEmpty(pixels);
      Assert.InRange((pixels.Min(p => p.x) + pixels.Max(p => p.x)) / 2f,
        bitmap.Width / 2f - 2, bitmap.Width / 2f + 2);
      Assert.InRange(pixels.Min(p => p.x), 1, bitmap.Width - 2);
      Assert.InRange(pixels.Max(p => p.x), 1, bitmap.Width - 2);
    }

    [Theory]
    [InlineData(600f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    public void TextPainterCentersTheCompleteEqnArrayAtFiniteAndUnboundedWidths(float width) {
      var painter = new SkiaSharp.TextPainter { FontSize = 32, LaTeX = EqnArrayText };
      var measure = painter.Measure(width);
      Assert.True(float.IsFinite(measure.Left) && float.IsFinite(measure.Top));
      Assert.True(float.IsFinite(measure.Width) && float.IsFinite(measure.Height));
      var display = Assert.Single(painter._absoluteXCoordDisplay.Displays);
      var bounds = DisplayInkBounds.Get(display).Plus(display.Position);
      if (float.IsFinite(width))
        Assert.Equal(width / 2, bounds.Left + bounds.Width / 2, precision: 3);
      else
        Assert.Equal(0, bounds.Left, precision: 3);
    }

    [Theory]
    [InlineData(TextAlignment.TopLeft)]
    [InlineData(TextAlignment.Center)]
    public void MathPainterMeasureThenDrawUsesTheSameAggregateBounds(TextAlignment alignment) {
      var painter = new SkiaSharp.MathPainter { FontSize = 32, LaTeX = Maxwell };
      var measured = painter.Measure();
      var width = Math.Max(1, (int)Math.Ceiling(measured.Width) + 4);
      var height = Math.Max(1, (int)Math.Ceiling(measured.Height) + 4);
      using var bitmap = new SKBitmap(width, height);
      using var canvas = new SKCanvas(bitmap);
      painter.Draw(canvas, alignment);
      var pixels = Enumerable.Range(0, bitmap.Width).SelectMany(x =>
        Enumerable.Range(0, bitmap.Height).Select(y => (x, y, a: bitmap.GetPixel(x, y).Alpha)))
        .Where(p => p.a > 0).ToArray();
      Assert.NotEmpty(pixels);
      Assert.InRange(pixels.Min(p => p.x), 1, bitmap.Width - 2);
      Assert.InRange(pixels.Max(p => p.x), 1, bitmap.Width - 2);
      Assert.True(measured.Width > 0 && measured.Height > 0);
      if (alignment == TextAlignment.Center)
        Assert.InRange((pixels.Min(p => p.x) + pixels.Max(p => p.x)) / 2f,
          bitmap.Width / 2f - 3, bitmap.Width / 2f + 3);
    }

    [Fact]
    public void OffsetAccentIsIncludedInAggregateInkBounds() {
      var painter = new SkiaSharp.MathPainter { FontSize = 32, LaTeX = @"\widehat{AB}" };
      painter.Measure();
      var root = Assert.IsType<Display.Displays.ListDisplay<Fonts, Glyph>>(painter.Display);
      var accent = Assert.Single(root.Displays.OfType<Display.Displays.AccentDisplay<Fonts, Glyph>>());
      var bounds = DisplayInkBounds.Get(root);
      var accentRight = accent.Position.X + accent.Accent.Position.X + accent.Accent.DisplayBounds().Width;
      Assert.InRange(bounds.Right, accentRight - 0.01f, float.PositiveInfinity);
    }

    [Fact]
    public void NonMultilineTextHighlightRetainsLegacyBounds() {
      var painter = new SkiaSharp.TextPainter {
        FontSize = 32, LaTeX = "single line", HighlightColor = SKColors.Red
      };
      painter.Measure(400);
      Assert.False(DisplayInkBounds.RequiresAggregateBounds(painter.Display!));
      using var bitmap = new SKBitmap(400, 100);
      using var canvas = new SKCanvas(bitmap);
      painter.Draw(canvas, TextAlignment.TopLeft);
    }

    [Fact]
    public void MultilineHighlightUsesAggregateBounds() {
      var painter = new SkiaSharp.MathPainter {
        FontSize = 32, LaTeX = Maxwell, HighlightColor = SKColors.Red
      };
      var measure = painter.Measure();
      Assert.True(DisplayInkBounds.RequiresAggregateBounds(painter.Display!));
      using var bitmap = new SKBitmap(Math.Max(1, (int)Math.Ceiling(measure.Width) + 4),
        Math.Max(1, (int)Math.Ceiling(measure.Height) + 4));
      using var canvas = new SKCanvas(bitmap);
      painter.Draw(canvas, TextAlignment.TopLeft);
    }
  }
}
