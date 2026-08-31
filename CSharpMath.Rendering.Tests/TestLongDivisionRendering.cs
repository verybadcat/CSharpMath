using System.IO;
using System.Linq;
using CSharpMath.Avalonia;
using CSharpMath.SkiaSharp;
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
  }
}
