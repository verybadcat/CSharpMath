using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using Rendering.FrontEnd;
  [CollectionDefinition(nameof(TestRenderingFixture))]
  public class TestRenderingFixture : ICollectionFixture<TestRenderingFixture> {
    public TestRenderingFixture() {
      Assert.NotEmpty(Folders);
      // Delete garbage by previous tests
      foreach (var garbage in
        Folders.SelectMany(folder =>
          Directory.EnumerateFiles(folder, "*.*.png")))
        File.Delete(garbage);
      // Pre-initialize typefaces to speed tests up
      BackEnd.Fonts.GlobalTypefaces.ToString();
      // Needed by Avalonia tests!
      global::Avalonia.Skia.SkiaPlatform.Initialize();
    }
    // https://www.codecogs.com/latex/eqneditor.php
    static string ThisFilePath
      ([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
      path ?? throw new ArgumentNullException(nameof(path));
    public static DirectoryInfo ThisDirectory = new FileInfo(ThisFilePath()).Directory;
    public static string GetFolder(string folderName) =>
      ThisDirectory.CreateSubdirectory(folderName).FullName;
    public static IEnumerable<string> Folders =>
      typeof(TestRenderingFixture)
      .Assembly
      .GetTypes()
      .Where(t => IsSubclassOfRawGeneric(typeof(TestRendering<,,,>), t))
      .SelectMany(t => t.GetMethods())
      .Where(method => method.IsDefined(typeof(FactAttribute), false)
                    || method.IsDefined(typeof(TheoryAttribute), false))
      .Select(method => GetFolder(method.Name))
      .Distinct();
    // https://stackoverflow.com/a/457708/5429648
    static bool IsSubclassOfRawGeneric(Type generic, Type? toCheck) {
      while (toCheck != null && toCheck != typeof(object)) {
        var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        if (generic == cur) {
          return true;
        }
        toCheck = toCheck.BaseType;
      }
      return false;
    }
    // https://stackoverflow.com/a/2637303/5429648
    public static bool StreamsContentsAreEqual(Stream stream1, Stream stream2) {
      const int bufferSize = 2048 * 2;
      var buffer1 = new byte[bufferSize];
      var buffer2 = new byte[bufferSize];
      while (true) {
        int count1 = stream1.Read(buffer1, 0, bufferSize);
        int count2 = stream2.Read(buffer2, 0, bufferSize);
        if (count1 != count2) return false;
        if (count1 == 0) return true;
        int iterations = (int)Math.Ceiling((double)count1 / sizeof(long));
        for (int i = 0; i < iterations; i++) {
          if (BitConverter.ToInt64(buffer1, i * sizeof(long)) != BitConverter.ToInt64(buffer2, i * sizeof(long))) {
            return false;
          }
        }
      }
    }
  }
  [Collection(nameof(TestRenderingFixture))]
  public abstract class TestRendering<TCanvas, TColor, TMathPainter, TTextPainter>
    where TMathPainter : MathPainter<TCanvas, TColor>, new() 
    where TTextPainter : TextPainter<TCanvas, TColor>, new() {
    protected abstract string FrontEnd { get; }
    /// <summary>Maximum percentage change from expected file size to actual file size * 100</summary>
    protected abstract double FileSizeTolerance { get; }
    protected abstract void DrawToStream<TContent>(Painter<TCanvas, TContent, TColor> painter, Stream stream, float textPainterCanvasWidth) where TContent : class;
    [Theory, ClassData(typeof(TestRenderingMathData))]
    public void Display(string file, string latex) =>
      Run(file, latex, nameof(Display), new TMathPainter { LineStyle = Atom.LineStyle.Display });
    [Theory, ClassData(typeof(TestRenderingMathData))]
    public void Inline(string file, string latex) =>
      Run(file, latex, nameof(Inline), new TMathPainter { LineStyle = Atom.LineStyle.Text });
    [Theory, ClassData(typeof(TestRenderingTextData))]
    public void Text(string file, string latex) =>
      Run(file, latex, nameof(Text), new TTextPainter());
    [Theory, ClassData(typeof(TestRenderingTextData))]
    public void TextInfiniteWidth(string file, string latex) =>
      Run(file, latex, nameof(TextInfiniteWidth), new TTextPainter(), float.PositiveInfinity);
    protected void Run<TContent>(
      string inFile, string latex, string folder, Painter<TCanvas, TContent, TColor> painter,
      float textPainterCanvasWidth = TextPainter<TCanvas, TColor>.DefaultCanvasWidth) where TContent : class {
      folder = TestRenderingFixture.GetFolder(folder);
      var frontEnd = FrontEnd.ToLowerInvariant();

      // Prevent black background behind black rendered output in File Explorer preview
      painter.HighlightColor = painter.UnwrapColor(new Structures.Color(0xF0, 0xF0, 0xF0));
      painter.LaTeX = latex;
      
      var actualFile = new FileInfo(System.IO.Path.Combine(folder, inFile + "." + frontEnd + ".png"));
      Assert.False(actualFile.Exists, $"The actual file was not deleted by test initialization: {actualFile.FullName}");

      using (var outFile = actualFile.OpenWrite())
        DrawToStream(painter, outFile, textPainterCanvasWidth);
      actualFile.Refresh();
      Assert.True(actualFile.Exists, "The actual image was not created successfully.");

      var expectedFile = new FileInfo(System.IO.Path.Combine(folder, inFile + ".png"));
      if (!expectedFile.Exists) {
        if (FileSizeTolerance != 0) return; // Only let SkiaSharp create the baseline
        actualFile.CopyTo(expectedFile.FullName);
        expectedFile.Refresh();
      }
      Assert.True(expectedFile.Exists, "The expected image was not copied successfully.");
      using var actualStream = actualFile.OpenRead();
      using var expectedStream = expectedFile.OpenRead();
      CoreTests.Approximately.Equal(expectedStream.Length, actualStream.Length, expectedStream.Length * FileSizeTolerance);
      if (FileSizeTolerance == 0)
        Assert.True(TestRenderingFixture.StreamsContentsAreEqual(expectedStream, actualStream), "The images differ.");
    }
    void PainterSettings<TPainter, TContent>(Action<string, TPainter> run) where TPainter : Painter<TCanvas, TContent, TColor>, new() where TContent : class {
      run("Baseline", new TPainter());
      run("Stroke", new TPainter { PaintStyle = PaintStyle.Stroke });
#warning For some reason the Avalonia front end behaves correctly for TextPainter Magnification test but not the SkiaSharp front end??
      //run("Magnification", new TPainter { Magnification = 2 });
      using var comicNeue = TestRenderingFixture.ThisDirectory.EnumerateFiles("ComicNeue_Bold.otf").Single().OpenRead();
      run("LocalTypeface", new TPainter {
        LocalTypefaces = new[] {
          new Typography.OpenFont.OpenFontReader().Read(comicNeue)
          ?? throw new Structures.InvalidCodePathException("Invalid font!")
        }
      });
      run("TextLineStyle", new TPainter { LineStyle = Atom.LineStyle.Text });
      run("ScriptLineStyle", new TPainter { LineStyle = Atom.LineStyle.Script });
      run("ScriptScriptLineStyle", new TPainter { LineStyle = Atom.LineStyle.ScriptScript });
      TColor green = new TPainter().UnwrapColor(Structures.Color.PredefinedColors[nameof(green)]);
      TColor blue = new TPainter().UnwrapColor(Structures.Color.PredefinedColors[nameof(blue)]);
      TColor orange = new TPainter().UnwrapColor(Structures.Color.PredefinedColors[nameof(orange)]);
      run("GlyphBoxColor", new TPainter { GlyphBoxColor = (green, blue) });
      run("TextColor", new TPainter { TextColor = orange });
    }
    protected void MathPainterSettingsTest<TContent>(string file, Painter<TCanvas, TContent, TColor> painter) where TContent : class =>
      Run(file, @"\sqrt[3]\frac\color{#F00}a\mathbb C", nameof(MathPainterSettings), painter);
    [Fact]
    public virtual void MathPainterSettings() =>
      PainterSettings<TMathPainter, Atom.MathList>(MathPainterSettingsTest);
    protected void TextPainterSettingsTest<TContent>(string file, Painter<TCanvas, TContent, TColor> painter) where TContent : class =>
      Run(file, @"Inline \color{red}{Maths}: $\int_{a_1^2}^{a_2^2}\color{green}\sqrt\frac x2dx$Display \color{red}{Maths}: $$\int_{a_1^2}^{a_2^2}\color{green}\sqrt\frac x2dx$$", nameof(TextPainterSettings), painter);
#warning Fix for CI
    [Fact(Skip="Awaiting fix for CI")]
    public virtual void TextPainterSettings() =>
      PainterSettings<TTextPainter, Text.TextAtom>(TextPainterSettingsTest);
  }
}
