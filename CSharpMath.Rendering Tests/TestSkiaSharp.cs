using System.Linq;
using SkiaSharp;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using FrontEnd;
  using SkiaSharp;
  public class TestSkiaSharp : Test<SKCanvas, SKColor, MathPainter, TextPainter> {
    protected override string FrontEnd => nameof(SkiaSharp);
    protected override double FileSizeTolerance => 0; // SkiaSharp is the baseline, no deviations allowed
    protected override void DrawToStream<TContent>(Painter<SKCanvas, TContent, SKColor> painter, System.IO.Stream stream) =>
      painter.DrawAsStream()?.CopyTo(stream);
    [Fact]
    public void MathPainterSettings() {
      void Test<TContent>(string file, Painter<SKCanvas, TContent, SKColor> painter) where TContent : class =>
        Run(file, @"\sqrt[3]\frac\color{#F00}a\mathbb C", nameof(MathPainterSettings), painter);
      Test("Baseline", new MathPainter());
      Test("Stroke", new MathPainter { PaintStyle = PaintStyle.Stroke });
      Test("Magnification1.5", new MathPainter { Magnification = 1.5f });
      Test("Magnification2", new MathPainter { Magnification = 2 });
      using var comicNeue = TestFixture.ThisDirectory.EnumerateFiles("ComicNeue_Bold.otf").Single().OpenRead();
      Test("LocalTypeface", new MathPainter {
        LocalTypefaces = {
          new Typography.OpenFont.OpenFontReader().Read(comicNeue)
          ?? throw new Structures.InvalidCodePathException("Invalid font!")
      }
      });
      Test("GlyphBoxColor", new MathPainter { GlyphBoxColor = (SKColors.Green, SKColors.Blue) });
      Test("TextColor", new MathPainter { TextColor = SKColors.Orange });
      Test("SquareStrokeCap", new MathPainter { StrokeCap = SKStrokeCap.Square });
      Test("RoundStrokeCap", new MathPainter { StrokeCap = SKStrokeCap.Round });
      Test("NoAntiAlias", new MathPainter { AntiAlias = false });
      Test("TextLineStyle", new MathPainter { LineStyle = Atom.LineStyle.Text });
      Test("ScriptLineStyle", new MathPainter { LineStyle = Atom.LineStyle.Script });
      Test("ScriptScriptLineStyle", new MathPainter { LineStyle = Atom.LineStyle.ScriptScript });
    }
  }
}
