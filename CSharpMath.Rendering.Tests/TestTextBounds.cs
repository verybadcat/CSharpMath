using System;
using System.Drawing;
using System.Linq;
using CSharpMath.Display;
using CSharpMath.Display.Displays;
using CSharpMath.Rendering.FrontEnd;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  public class TestTextBounds {
    const float Width = 880;
    const string Italian = "Se di fronte a te si palesasse un acido grasso extra-large, lo riconosceresti immediatamente perché la sua natura fisica è data dalla lunghezza del tuo scheletro carbonioso";

    [Fact]
    public void RightAlignmentRetainsTypographicRightShiftFormula() {
      var position = IPainterExtensions.GetDisplayPosition(
        displayWidth: 240, displayAscent: 40, displayDescent: 10,
        fontSize: 48, width: 880, height: 600,
        alignment: TextAlignment.TopRight,
        padding: new Thickness(17, 0, 23, 0), offsetX: 5, offsetY: 0);
      Assert.Equal(622, position.X, precision: 4);
    }

    static (SkiaSharp.TextPainter painter, RectangleF measure, SKBitmap bitmap) Draw(string text, TextAlignment alignment, Thickness padding = default, float offsetX = 0) {
      var painter = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = text };
      var measure = painter.Measure(ConstrainedTextLayout.ContentWidth(Width + 120, padding.Left, padding.Right));
      var bitmap = new SKBitmap((int)Width + 120, (int)Math.Ceiling(measure.Height + 100));
      using var canvas = new SKCanvas(bitmap);
      canvas.Clear(SKColors.Transparent);
      painter.Draw(canvas, new RectangleF(offsetX, 0, bitmap.Width, bitmap.Height), alignment, padding);
      return (painter, measure, bitmap);
    }

    static RectangleF InkBounds(SkiaSharp.TextPainter painter) {
      var bounds = RectangleF.Empty;
      foreach (var display in painter._relativeXCoordDisplay.Displays) {
        var ink = display is TextRunDisplay<BackEnd.Fonts, BackEnd.Glyph> run
          ? run.InkBounds
          : display.DisplayBounds();
        ink = ink.Plus(display.Position).Plus(painter._relativeXCoordDisplay.Position);
        bounds = bounds.IsEmpty ? ink : bounds.Union(ink);
      }
      return bounds;
    }

    [Fact]
    public void CenteredBreakableTextKeepsInkInsideFiniteBoxAndMeasureCoversIt() {
      var (_, measure, bitmap) = Draw(Italian, TextAlignment.Top, new Thickness(60, 0, 60, 0));
      var rendered = AssertRenderedRowsInside(bitmap, 60, Width + 60);
      Assert.True(rendered.Width <= measure.Width + 2);
      var rows = Enumerable.Range(0, bitmap.Height)
        .Select(y => (y, xs: Enumerable.Range(60, (int)Width).Where(x => bitmap.GetPixel(x, y).Alpha > 0).ToArray()))
        .Where(r => r.xs.Length > 0).ToList();
      for (var i = 0; i < rows.Count;) {
        var j = i + 1;
        while (j < rows.Count && rows[j].y <= rows[j - 1].y + 1) j++;
        var line = rows.GetRange(i, j - i);
        var min = line.Min(r => r.xs[0]);
        var max = line.Max(r => r.xs[^1]);
        Assert.InRange((min + max) / 2f, 480, 530);
        i = j;
      }
    }

    [Theory]
    [InlineData(TextAlignment.TopLeft)]
    [InlineData(TextAlignment.TopRight)]
    public void LeftAndRightAlignmentRemainWithinTheirExpectedFiniteBox(TextAlignment alignment) {
      var (_, _, bitmap) = Draw("Italic overhangs should not alter line grouping.", alignment, new Thickness(60, 0, 60, 0));
      var bounds = AssertRenderedRowsInside(bitmap, 0, bitmap.Width);
      if (alignment == TextAlignment.TopLeft) Assert.InRange(bounds.Left, 58, 62);
      else Assert.InRange(bounds.Right, Width + 45, Width + 60);
    }

    [Fact]
    public void UnbreakableTextMayOverflowFiniteBox() {
      var painter = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = new string('W', 100) };
      var natural = painter.Measure(float.PositiveInfinity);
      using var bitmap = new SKBitmap((int)Math.Ceiling(natural.Width) + 120, (int)Math.Ceiling(natural.Height) + 100);
      using var canvas = new SKCanvas(bitmap);
      canvas.Clear(SKColors.Transparent);
      painter.Draw(canvas, top: 0, left: 60, right: 940);
      var bounds = AssertRenderedRowsInside(bitmap, 0, bitmap.Width);
      Assert.True(natural.Width > Width);
      Assert.True(bounds.Right > 940);
      Assert.True(bounds.Right < bitmap.Width - 10);
    }

    [Fact]
    public void CenteredTextHonorsAsymmetricPaddingAndOffset() {
      var (_, _, bitmap) = Draw(Italian, TextAlignment.Top,
        new Thickness(37, 0, 113, 0), 19);
      AssertRenderedRowsInside(bitmap, 56, Width + 120 - 113 + 19);
    }

    [Fact]
    public void ExplicitBoundedDrawUsesTheSuppliedRegion() {
      var painter = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = Italian };
      using var bitmap = new SKBitmap(1000, 1000);
      using var canvas = new SKCanvas(bitmap);
      canvas.Clear(SKColors.Transparent);
      painter.Draw(canvas, new RectangleF(60, 0, 880, 1000), TextAlignment.Top);
      var bounds = AssertRenderedRowsInside(bitmap, 60, 940);
      Assert.True(bounds.Width > 0);
    }

    [Fact]
    public void LegacyDrawRetainsOriginalPaddingGeometry() {
      const string shortText = "short legacy text";
      var legacy = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = shortText };
      var constrained = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = shortText };
      using var legacyBitmap = new SKBitmap(1000, 1000);
      using var constrainedBitmap = new SKBitmap(1000, 1000);
      using var legacyCanvas = new SKCanvas(legacyBitmap);
      using var constrainedCanvas = new SKCanvas(constrainedBitmap);
      legacyCanvas.Clear(SKColors.Transparent);
      constrainedCanvas.Clear(SKColors.Transparent);
      // With no padding and top-left alignment, the explicit region is a
      // semantic counterpart of the legacy canvas-width call.
      legacy.Draw(legacyCanvas, TextAlignment.Top);
      constrained.Draw(constrainedCanvas, new RectangleF(0, 0, 1000, 1000), TextAlignment.Top);
      Assert.InRange(Math.Abs(legacy._relativeXCoordDisplay.Position.X - constrained._relativeXCoordDisplay.Position.X), 0, 1);
      Assert.InRange(Math.Abs(legacy._relativeXCoordDisplay.Position.Y - constrained._relativeXCoordDisplay.Position.Y), 0, 1);
      AssertInkBoundsClose(InkBounds(legacy), InkBounds(constrained));
    }

    static void AssertInkBoundsClose(RectangleF expected, RectangleF actual) {
      Assert.InRange(Math.Abs(expected.Left - actual.Left), 0, 1);
      Assert.InRange(Math.Abs(expected.Top - actual.Top), 0, 1);
      Assert.InRange(Math.Abs(expected.Right - actual.Right), 0, 1);
      Assert.InRange(Math.Abs(expected.Bottom - actual.Bottom), 0, 1);
    }

    [Fact]
    public void ExplicitRegionHonorsTranslatedOriginAndContentBox() {
      var painter = new SkiaSharp.TextPainter { FontSize = 48, LaTeX = "translated centered text" };
      using var bitmap = new SKBitmap(1000, 1000);
      using var displacedBitmap = new SKBitmap(1000, 1000);
      using var canvas = new SKCanvas(bitmap);
      using var displacedCanvas = new SKCanvas(displacedBitmap);
      canvas.Clear(SKColors.Transparent);
      displacedCanvas.Clear(SKColors.Transparent);
      var region = new RectangleF(37, 53, 880, 700);
      painter.Draw(canvas, new RectangleF(37, 53, 880, 700), TextAlignment.Top,
        new Thickness(41, 17, 73, 29));
      painter.Draw(displacedCanvas, region, TextAlignment.Top,
        new Thickness(41, 17, 73, 29), offsetY: 24);
      var bounds = AssertRenderedRowsInside(bitmap, 78, 844);
      var displacedBounds = AssertRenderedRowsInside(displacedBitmap, 78, 844);
      Assert.True(bounds.Width <= painter.Measure(766).Width + 2);
      Assert.True(bounds.Left >= 77 && bounds.Right <= 845);
      Assert.Equal(bounds.Left, displacedBounds.Left);
      Assert.Equal(bounds.Right, displacedBounds.Right);
      Assert.InRange(displacedBounds.Top - bounds.Top, 23, 25);
      Assert.InRange(displacedBounds.Bottom - bounds.Bottom, 23, 25);
    }

    static RectangleF AssertRenderedRowsInside(SKBitmap bitmap, float left, float right) {
      var result = RectangleF.Empty;
      for (var y = 0; y < bitmap.Height; y++) {
        var xs = Enumerable.Range(0, bitmap.Width)
          .Where(x => bitmap.GetPixel(x, y).Alpha > 0)
          .ToArray();
        if (xs.Length == 0) continue;
        var row = new RectangleF(xs[0], y, xs[^1] - xs[0] + 1, 1);
        result = result.IsEmpty ? row : result.Union(row);
        Assert.True(xs[0] >= left - 1, $"row {y} starts at {xs[0]}");
        Assert.True(xs[^1] <= right + 1, $"row {y} ends at {xs[^1]}");
      }
      Assert.False(result.IsEmpty);
      Assert.True(result.Bottom < bitmap.Height - 2);
      return result;
    }
  }
}
