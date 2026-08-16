using Xunit;

namespace CSharpMath.Rendering.Tests {
  /// <summary>
  /// <c>\bmod</c> is the first binary operator here whose nucleus is a word rather than a symbol,
  /// and <c>\operatorname{φ}</c> the first operator name written with a non-ASCII letter. Both come
  /// from AngouriMath. Glyph coverage is already asserted by
  /// <see cref="TestCommandDisplay.CommandsAreDisplayable"/>; what is new is the layout, so this
  /// measures rather than comparing against a baseline image.
  /// </summary>
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
  }
}
