using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using System.Runtime.CompilerServices;
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
    static string ThisFilePath([CallerFilePath] string? path = null) =>
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
      .Where(method => method.IsDefined(typeof(SkippableFactAttribute), false)
                    || method.IsDefined(typeof(SkippableTheoryAttribute), false))
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
    /// <summary>Maximum percentage change from expected file size to actual file size / 100</summary>
    protected abstract double FileSizeTolerance { get; }
    protected abstract void DrawToStream<TContent>(Painter<TCanvas, TContent, TColor> painter,
      Stream stream, float textPainterCanvasWidth, TextAlignment alignment) where TContent : class;
    [SkippableTheory, ClassData(typeof(TestRenderingMathData))]
    public void MathDisplay(string file, string latex) =>
      Run(file, latex, new TMathPainter { LineStyle = Atom.LineStyle.Display });
    [SkippableTheory, ClassData(typeof(TestRenderingMathData))]
    public void MathInline(string file, string latex) =>
      Run(file, latex, new TMathPainter { LineStyle = Atom.LineStyle.Text });
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextLeft(string file, string latex) =>
      Run(file, latex, new TTextPainter());
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextCenter(string file, string latex) =>
      Run(file, latex, new TTextPainter(), TextAlignment.Top);
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextRight(string file, string latex) =>
      Run(file, latex, new TTextPainter(), TextAlignment.TopRight);
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextLeftInfiniteWidth(string file, string latex) =>
      Run(file, latex, new TTextPainter(), textPainterCanvasWidth: float.PositiveInfinity);
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextCenterInfiniteWidth(string file, string latex) =>
      Run(file, latex, new TTextPainter(), TextAlignment.Top, textPainterCanvasWidth: float.PositiveInfinity);
    [SkippableTheory, ClassData(typeof(TestRenderingTextData))]
    public void TextRightInfiniteWidth(string file, string latex) =>
      Run(file, latex, new TTextPainter(), TextAlignment.TopRight, textPainterCanvasWidth: float.PositiveInfinity);
    protected void Run<TContent>(
      string inFile, string latex, Painter<TCanvas, TContent, TColor> painter, TextAlignment alignment = TextAlignment.TopLeft,
      float textPainterCanvasWidth = TextPainter<TCanvas, TColor>.DefaultCanvasWidth, [CallerMemberName]string folder = "") where TContent : class {
      folder = TestRenderingFixture.GetFolder(folder);
      var frontEnd = FrontEnd.ToLowerInvariant();

      // Prevent black background behind black rendered output in File Explorer preview
      painter.HighlightColor = painter.UnwrapColor(System.Drawing.Color.FromArgb(0xF0, 0xF0, 0xF0));
      painter.LaTeX = latex;

      var actualFile = new FileInfo(System.IO.Path.Combine(folder, inFile + "." + frontEnd + ".png"));
      Assert.False(actualFile.Exists, $"The actual file was not deleted by test initialization: {actualFile.FullName}");

      using (var outFile = actualFile.OpenWrite())
        DrawToStream(painter, outFile, textPainterCanvasWidth, alignment);
      actualFile.Refresh();
      Assert.True(actualFile.Exists, "The actual image was not created successfully.");

      var expectedFile = new FileInfo(System.IO.Path.Combine(folder, inFile + ".png"));
      if (!expectedFile.Exists) {
        Skip.If(FileSizeTolerance != 0, "Baseline images may only be created by SkiaSharp.");
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
    public static TheoryData<string, TPainter> PainterSettingsData<TPainter, TContent>() where TPainter : Painter<TCanvas, TContent, TColor>, new() where TContent : class =>
      new TheoryData<string, TPainter> {
        { "Baseline", new TPainter() },
        { "Stroke", new TPainter { PaintStyle = PaintStyle.Stroke } },
#warning For some reason the Avalonia front end behaves correctly for TextPainter Magnification test but not the SkiaSharp front end??
        { "Magnification", new TPainter { Magnification = 2 } },
#warning For some reason SkiaSharp produces an erroneous image only on Ubuntu??
        //{ "LocalTypeface", new TPainter {
        //  LocalTypefaces = new[] {
        //    new Typography.OpenFont.OpenFontReader().Read(
        //      TestRenderingFixture.ThisDirectory.EnumerateFiles("ComicNeue_Bold.otf").Single().OpenRead()
        //    ) ?? throw new Structures.InvalidCodePathException("Invalid font!")
        //  }
        //} },
        { "TextLineStyle", new TPainter { LineStyle = Atom.LineStyle.Text } },
        { "ScriptLineStyle", new TPainter { LineStyle = Atom.LineStyle.Script } },
        { "ScriptScriptLineStyle", new TPainter { LineStyle = Atom.LineStyle.ScriptScript } },
        { "GlyphBoxColor", new TPainter { GlyphBoxColor = (
          new TPainter().UnwrapColor(Structures.ColorExtensions.PredefinedColors["green"]),
          new TPainter().UnwrapColor(Structures.ColorExtensions.PredefinedColors["blue"])
        ) } },
        { "TextColor", new TPainter { TextColor =
          new TPainter().UnwrapColor(Structures.ColorExtensions.PredefinedColors["orange"]) } },
    };
    public static TheoryData<string, TMathPainter> MathPainterSettingsData => PainterSettingsData<TMathPainter, Atom.MathList>();
    public static TheoryData<string, TTextPainter> TextPainterSettingsData => PainterSettingsData<TTextPainter, Text.TextAtom>();
    [SkippableTheory]
    [MemberData(nameof(MathPainterSettingsData))]
    public virtual void MathPainterSettings(string file, TMathPainter painter) =>
      Run(file, @"\sqrt[3]\frac\color{#F00}a\mathbb C", painter);
    [SkippableTheory]
    [MemberData(nameof(TextPainterSettingsData))]
    public void TextPainterSettings(string file, TTextPainter painter) =>
      Run(file, @"Inline \color{red}{Maths}: $\int_{a_1^2}^{a_2^2}\color{green}\sqrt\frac x2dx$Display \color{red}{Maths}: $$\int_{a_1^2}^{a_2^2}\color{green}\sqrt\frac x2dx$$", painter);
  }
}
