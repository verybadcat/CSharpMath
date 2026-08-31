using System;
using System.Linq;
using Xunit;
using SkiaSharp;

namespace CSharpMath.Rendering.Tests {
  /// <summary>
  /// <c>\bmod</c> is the first binary operator here whose nucleus is a word rather than a symbol,
  /// and <c>\operatorname{φ}</c> the first operator name written with a non-ASCII letter. Both come
  /// from AngouriMath. Glyph coverage is already asserted by
  /// <see cref="TestCommandDisplay.CommandsAreDisplayable"/>; what is new is the layout, so this
  /// measures rather than comparing against a baseline image.
  /// </summary>
  [Collection(nameof(TestRenderingFixture))]
  public class TestAngouriMathForms {
    static System.Drawing.RectangleF Measure(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      Assert.Null(painter.ErrorMessage);
      return painter.Measure(FrontEnd.TextPainter<global::SkiaSharp.SKCanvas, global::SkiaSharp.SKColor>.DefaultCanvasWidth);
    }

    [Theory]
    [InlineData(@"x\bmod y")]
    [InlineData(@"\operatorname{φ}\left( x\right) ")]
    [InlineData(@"\operatorname{φ}\left( x\bmod y\right) ")]
    public void TheyLayOut(string latex) {
      var measured = Measure(latex);
      Assert.True(measured.Width > 0, $"zero width for {latex}");
      Assert.True(measured.Height > 0, $"zero height for {latex}");
    }

    /// <summary>
    /// The three letters of "mod" and the spacing around a binary operator are actually laid out,
    /// rather than the atom occupying no room -- which a zero-width check alone would not catch,
    /// since x and y are there either way.
    /// </summary>
    [Fact]
    public void ModuloTakesUpRoom() =>
      Assert.True(Measure(@"x\bmod y").Width > Measure(@"xy").Width * 2);

    [Fact]
    public void MathRelAndJoinRelMeasureWithoutClipping() {
      var wrapped = Measure(@"a\mathrel{|}\joinrel=b");
      var relation = Measure("a=b");
      Assert.True(wrapped.Width > 0 && wrapped.Height > 0);
      Assert.True(relation.Width > 0 && relation.Height > 0);
      Assert.Null(new SkiaSharp.MathPainter { LaTeX = @"\mathrel{\left( x\right)}" }.ErrorMessage);
    }

    [Theory]
    [InlineData(@"\joinrel x")]
    [InlineData(@"x\joinrel")]
    [InlineData(@"\mathrel{\left(\joinrel x\right)}")]
    [InlineData(@"\frac{\joinrel x}{x}")]
    [InlineData(@"\sqrt{\joinrel x}")]
    [InlineData(@"\bar{\joinrel x}")]
    public void SkiaTightCanvasContainsInkAtBothEdges(string latex) {
      var painter = new SkiaSharp.MathPainter { LaTeX = latex };
      var measure = painter.Measure(1000);
      // Allocate only the measured ink span, with a one-pixel safety edge. This catches
      // both clipping and accidental double compensation of a negative ink origin.
      var width = Math.Max(1, (int)Math.Ceiling(measure.Width) + 2);
      var height = Math.Max(1, (int)Math.Ceiling(measure.Height) + 2);
      using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
      using var canvas = new SKCanvas(bitmap);
      canvas.Clear(SKColors.Transparent);
      painter.Draw(canvas, 1, height - 1 - painter.Display!.Descent);
      var occupied = Enumerable.Range(0, width).Where(x =>
        Enumerable.Range(0, height).Any(y => bitmap.GetPixel(x, y).Alpha > 0)).ToArray();
      Assert.NotEmpty(occupied);
      Assert.True(occupied.First() >= 1);
      Assert.True(occupied.Last() <= width - 2);
      Assert.True(occupied.Last() > occupied.First());
    }

    [Theory]
    [InlineData(@"\joinrel x")]
    [InlineData(@"x\joinrel")]
    public void DirectBoundaryJoinRelMatchesPlainX(string latex) {
      static (int width, int height, int left, int right, int top, int bottom) Render(string source) {
        var painter = new SkiaSharp.MathPainter { LaTeX = source };
        var measure = painter.Measure(1000);
        var width = Math.Max(1, (int)Math.Ceiling(measure.Width) + 2);
        var height = Math.Max(1, (int)Math.Ceiling(measure.Height) + 2);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        painter.Draw(canvas, 1, height - 1 - painter.Display!.Descent);
        var pixels = Enumerable.Range(0, width)
          .SelectMany(x => Enumerable.Range(0, height).Select(y => (x, y)))
          .Where(p => bitmap.GetPixel(p.x, p.y).Alpha > 0).ToArray();
        Assert.NotEmpty(pixels);
        return (width, height, pixels.Min(p => p.x), pixels.Max(p => p.x),
          pixels.Min(p => p.y), pixels.Max(p => p.y));
      }
      Assert.Equal(Render("x"), Render(latex));
    }
  }
}
