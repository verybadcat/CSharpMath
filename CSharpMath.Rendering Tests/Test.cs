using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using Rendering.FrontEnd;
  [CollectionDefinition(nameof(TestFixture))]
  public class TestFixture : ICollectionFixture<TestFixture> {
    public TestFixture() {
      Assert.NotEmpty(Folders);
      // Delete garbage by previous tests
      foreach (var garbage in
        Folders.SelectMany(folder =>
          Directory.EnumerateFiles(folder)
          .Where(file => file.Contains(".avalonia.") || file.Contains(".skiasharp."))))
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
      typeof(TestFixture)
      .Assembly
      .GetTypes()
      .Where(t => IsSubclassOfRawGeneric(typeof(Test<,,,>), t))
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
  }
  [Collection(nameof(TestFixture))]
  public abstract class Test<TCanvas, TColor, TMathPainter, TTextPainter>
    where TMathPainter : MathPainter<TCanvas, TColor>, new() 
    where TTextPainter : TextPainter<TCanvas, TColor>, new() {
    protected abstract string FrontEnd { get; }
    /// <summary>Maximum percentage change from expected file size to actual file size * 100</summary>
    protected abstract double FileSizeTolerance { get; }
    protected abstract void DrawToStream<TContent>(Painter<TCanvas, TContent, TColor> painter, Stream stream) where TContent : class;
    [Theory, ClassData(typeof(MathData))]
    public void Display(string file, string latex) =>
      Run(file, latex, nameof(Display), new TMathPainter { LineStyle = Atom.LineStyle.Display });
    [Theory, ClassData(typeof(MathData))]
    public void Inline(string file, string latex) =>
      Run(file, latex, nameof(Inline), new TMathPainter { LineStyle = Atom.LineStyle.Text });
    [Theory, ClassData(typeof(TextData))]
    public void Text(string file, string latex) =>
      Run(file, latex, nameof(Text), new TTextPainter());
    protected void Run<TContent>(
      string inFile, string latex, string folder, Painter<TCanvas, TContent, TColor> painter) where TContent : class {
      folder = TestFixture.GetFolder(folder);
      var frontEnd = FrontEnd.ToLowerInvariant();

      // Prevent black background behind black rendered output in File Explorer preview
      painter.HighlightColor = painter.UnwrapColor(new Structures.Color(0xF0, 0xF0, 0xF0));
      painter.FontSize = 50f;
      painter.LaTeX = latex;
      if (painter.ErrorMessage != null)
        throw new Xunit.Sdk.XunitException("Painter error: " + painter.ErrorMessage);

      var actualFile = new FileInfo(Path.Combine(folder, inFile + "." + frontEnd + ".png"));
      Assert.False(actualFile.Exists, $"The actual file was not deleted by test initialization: {actualFile.FullName}");

      using (var outFile = actualFile.OpenWrite())
        DrawToStream(painter, outFile);
      actualFile.Refresh();
      Assert.True(actualFile.Exists, "The actual image was not created successfully.");

      var expectedFile = new FileInfo(Path.Combine(folder, inFile + ".png"));
      if (!expectedFile.Exists) {
        if (FileSizeTolerance != 0) return; // Only let SkiaSharp create the baseline
        actualFile.CopyTo(expectedFile.FullName);
        expectedFile.Refresh();
      }
      Assert.True(expectedFile.Exists, "The expected image was not copied successfully.");
      using var actualStream = actualFile.OpenRead();
      using var expectedStream = expectedFile.OpenRead();
      CSharpMath.Tests.Approximately.Equal(expectedStream.Length, actualStream.Length, expectedStream.Length * FileSizeTolerance);
      if (FileSizeTolerance == 0)
        Assert.True(StreamsContentsAreEqual(expectedStream, actualStream), "The images differ.");

      // https://stackoverflow.com/a/2637303/5429648
      static bool StreamsContentsAreEqual(Stream stream1, Stream stream2) {
        const int bufferSize = 2048 * 2;
        var buffer1 = new byte[bufferSize];
        var buffer2 = new byte[bufferSize];

        while (true) {
          int count1 = stream1.Read(buffer1, 0, bufferSize);
          int count2 = stream2.Read(buffer2, 0, bufferSize);

          if (count1 != count2) {
            return false;
          }

          if (count1 == 0) {
            return true;
          }

          int iterations = (int)Math.Ceiling((double)count1 / sizeof(long));
          for (int i = 0; i < iterations; i++) {
            if (BitConverter.ToInt64(buffer1, i * sizeof(long)) != BitConverter.ToInt64(buffer2, i * sizeof(long))) {
              return false;
            }
          }
        }
      }
    }
  }
}
