using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using Rendering;
  public class TestFixture {
    public TestFixture() {
      Assert.NotEmpty(Test.Folders);
      // Delete garbage by previous tests
      foreach (var garbage in
        Test.Folders.SelectMany(folder =>
          Directory.EnumerateFiles(folder)
          .Where(file => file.Contains(".avalonia.") || file.Contains(".skiasharp."))))
        File.Delete(garbage);
      // Pre-initialize to speed tests up
      global::Avalonia.Skia.SkiaPlatform.Initialize();
      BackEnd.Fonts.GlobalTypefaces.ToString();
    }
  }
  [CollectionDefinition(nameof(TestFixture))]
  public class TestFixtureCollection : ICollectionFixture<TestFixture> { }
  public static class Test {
    // https://www.codecogs.com/latex/eqneditor.php
    static string ThisFilePath
      ([System.Runtime.CompilerServices.CallerFilePath] string? path = null) =>
      path ?? throw new ArgumentNullException(nameof(path));
    public static DirectoryInfo ThisDirectory = new FileInfo(ThisFilePath()).Directory;
    public static string GetFolder(string folderName) =>
      ThisDirectory.CreateSubdirectory(folderName).FullName;
    public static IEnumerable<string> Folders =>
      new[] { typeof(TestAvalonia), typeof(TestSkiaSharp) }
      .SelectMany(t => t.GetMethods())
      .Where(method => method.IsDefined(typeof(FactAttribute), false)
                    || method.IsDefined(typeof(TheoryAttribute), false))
      .Select(method => GetFolder(method.Name))
      .Distinct();

    public static void Run<TCanvas, TContent, TColor>(
      string inFile, string latex, string folder, string frontEnd,
      Painter<TCanvas, TContent, TColor> painter,
      Action<Painter<TCanvas, TContent, TColor>, Stream> drawToStream) where TContent : class {
      folder = GetFolder(folder);
      frontEnd = frontEnd.ToLowerInvariant();

      // Prevent black background behind black rendered output in File Explorer preview
      painter.HighlightColor = painter.UnwrapColor(new Structures.Color(0xF0, 0xF0, 0xF0));
      painter.FontSize = 50f;
      painter.LaTeX = latex;
      if (painter.ErrorMessage != null)
        throw new Xunit.Sdk.XunitException("Painter error: " + painter.ErrorMessage);

      var actualFile = new FileInfo(Path.Combine(folder, inFile + "." + frontEnd + ".png"));
      Assert.False(actualFile.Exists, $"The actual file was not deleted by test initialization: {actualFile.FullName}");

      using (var outFile = actualFile.OpenWrite())
        drawToStream(painter, outFile);
      actualFile.Refresh();
      Assert.True(actualFile.Exists, "The actual image was not created successfully.");

      var expectedFile = new FileInfo(Path.Combine(folder, inFile + ".png"));
      if (!expectedFile.Exists) {
        actualFile.CopyTo(expectedFile.FullName);
        expectedFile.Refresh();
      }
      Assert.True(expectedFile.Exists, "The expected image was not copied successfully.");
      using var actualStream = actualFile.OpenRead();
      using var expectedStream = expectedFile.OpenRead();
      Assert.Equal(expectedStream.Length, actualStream.Length);
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
