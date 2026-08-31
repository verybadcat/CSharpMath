using System;
using System.IO;
using System.Linq;
using CSharpMath.Avalonia;
using CSharpMath.SkiaSharp;
using SkiaSharp;
using Xunit;
using AvaloniaMathPainter = CSharpMath.Avalonia.MathPainter;
using SkiaMathPainter = CSharpMath.SkiaSharp.MathPainter;

namespace CSharpMath.Rendering.Tests {
  public sealed class TestLongDivisionRendering {
    [Theory]
    [InlineData("12345", "13")]
    [InlineData("1005", "5")]
    [InlineData("3", "5")]
    [InlineData("0", "7")]
    [InlineData("9999999999999999999999999999", "1")]
    public void SkiaProducesMeasuredLongDivision(string numerator, string denominator) {
      using var stream = new SkiaMathPainter { LaTeX = $@"\longdiv{{{numerator}}}{{{denominator}}}" }.DrawAsStream();
      Assert.NotNull(stream);
      Assert.True(stream!.Length > 0);
    }

    [Theory]
    [InlineData("12345", "13")]
    [InlineData("1005", "5")]
    [InlineData("3", "5")]
    [InlineData("0", "7")]
    [InlineData("9999999999999999999999999999", "1")]
    public void AvaloniaProducesMeasuredLongDivision(string numerator, string denominator) {
      global::Avalonia.Skia.SkiaPlatform.Initialize();
      using var stream = new MemoryStream();
      new AvaloniaMathPainter {
        LaTeX = $@"\longdiv{{{numerator}}}{{{denominator}}}"
      }.DrawAsPng(stream);
      Assert.True(stream.Length > 0);
    }

    [Fact]
    public void ScalingChangesGeometryWithoutChangingSemanticRows() {
      var painter = new SkiaMathPainter { LaTeX = @"\longdiv{12345}{13}" };
      var normal = painter.Measure(2000);
      painter.FontSize *= 2;
      var large = painter.Measure(2000);
      Assert.True(large.Width > normal.Width);
      Assert.True(large.Height > normal.Height);
      var result = CSharpMath.Atom.LaTeXParser.MathListFromLaTeX(@"\longdiv{12345}{13}");
      Assert.Null(result.Error);
      var (list, _) = result;
      var atom = Assert.IsType<CSharpMath.Atom.Atoms.LongDivision>(list[0]);
      Assert.Equal(new[] { 0, 1, 2, 3, 4 }, atom.Steps.Select(s => s.DecimalColumn));
    }

    [Theory]
    [InlineData(@"\longdiv{12345}{13}", "display")]
    [InlineData(@"\text{answer: }\longdiv{1005}{5}", "text")]
    [InlineData(@"x_{\longdiv{3}{5}}", "script")]
    public void DisplayTextAndScriptRemainNonEmptyWhenScaled(string latex, string kind) {
      var painter = new SkiaMathPainter { LaTeX = latex };
      var before = painter.Measure(2000);
      painter.FontSize *= 0.5f;
      var half = painter.Measure(2000);
      painter.FontSize *= 4;
      var twice = painter.Measure(2000);
      Assert.True(before.Width > 0 && before.Height > 0, kind);
      Assert.True(half.Width > 0 && half.Height > 0, kind);
      Assert.True(twice.Width > half.Width && twice.Height > half.Height, kind);
    }

    [Theory]
    [InlineData(@"\longdiv{12345}{13}", CSharpMath.Atom.LineStyle.Text)]
    [InlineData(@"\longdiv{12345}{13}", CSharpMath.Atom.LineStyle.Script)]
    public void LongDivisionBarConnectsToDelimiterInTextAndScriptStyles(string latex, CSharpMath.Atom.LineStyle style) {
      using var skia = RenderSkia(latex, style);
      AssertBarConnectsToDelimiter(skia, latex);
      global::Avalonia.Skia.SkiaPlatform.Initialize();
      using var avalonia = new MemoryStream();
      new AvaloniaMathPainter { LaTeX = latex, LineStyle = style }.DrawAsPng(avalonia);
      avalonia.Position = 0;
      AssertBarConnectsToDelimiter(avalonia, latex);
    }

    private static MemoryStream RenderSkia(string latex, CSharpMath.Atom.LineStyle style) {
      using var rendered = new SkiaMathPainter { LaTeX = latex, LineStyle = style }.DrawAsStream();
      var copy = new MemoryStream();
      rendered!.CopyTo(copy);
      copy.Position = 0;
      return copy;
    }

    private static void AssertBarConnectsToDelimiter(Stream png, string latex) {
      using var bitmap = SKBitmap.Decode(png);
      Assert.NotNull(bitmap);
      var dark = new bool[bitmap!.Width, bitmap.Height];
      for (var y = 0; y < bitmap.Height; y++)
        for (var x = 0; x < bitmap.Width; x++) {
          var color = bitmap.GetPixel(x, y);
          dark[x, y] = color.Alpha > 32 && color.Red < 190 && color.Green < 190 && color.Blue < 190;
        }

      var bestY = -1;
      var bestStart = 0;
      var bestLength = 0;
      for (var y = 0; y < bitmap.Height; y++) {
        var start = -1;
        for (var x = 0; x <= bitmap.Width; x++) {
          if (x < bitmap.Width && dark[x, y]) {
            if (start < 0) start = x;
          } else if (start >= 0) {
            if (x - start > bestLength) {
              bestY = y;
              bestStart = start;
              bestLength = x - start;
            }
            start = -1;
          }
        }
      }
      Assert.True(bestLength >= 8, $"No overbar found for {latex}.");

      var ruleBottom = bestY;
      for (var y = bestY + 1; y < bitmap.Height; y++) {
        var run = LongestDarkRun(dark, bitmap.Width, y);
        if (run.Length < bestLength * 0.8) break;
        ruleBottom = y;
      }

      // The connected component must reach below the rule near its left endpoint.
      var visited = new bool[bitmap.Width, bitmap.Height];
      var pending = new System.Collections.Generic.Queue<(int X, int Y)>();
      for (var x = bestStart; x < bestStart + bestLength; x++) {
        visited[x, bestY] = true;
        pending.Enqueue((x, bestY));
      }
      var reachesBelow = false;
      while (pending.Count > 0) {
        var (x, y) = pending.Dequeue();
        if (y > ruleBottom + Math.Max(3, bitmap.Height / 12) && x <= bestStart + Math.Max(8, bitmap.Height / 8)) reachesBelow = true;
        foreach (var (nx, ny) in new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) })
          if (nx >= 0 && nx < bitmap.Width && ny >= 0 && ny < bitmap.Height && dark[nx, ny] && !visited[nx, ny]) {
            visited[nx, ny] = true;
            pending.Enqueue((nx, ny));
          }
      }
      Assert.True(reachesBelow, $"Overbar is not connected to the delimiter for {latex}.");
    }

    private static (int Start, int Length) LongestDarkRun(bool[,] dark, int width, int y) {
      var bestStart = 0;
      var bestLength = 0;
      var start = -1;
      for (var x = 0; x <= width; x++) {
        if (x < width && dark[x, y]) {
          if (start < 0) start = x;
        } else if (start >= 0) {
          if (x - start > bestLength) (bestStart, bestLength) = (start, x - start);
          start = -1;
        }
      }
      return (bestStart, bestLength);
    }
  }
}
