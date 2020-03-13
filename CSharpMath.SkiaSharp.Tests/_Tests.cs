using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.SkiaSharp {
  public static class Tests {
    // https://www.codecogs.com/latex/eqneditor.php
    static string ThisFilePath
      ([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;
    static string GetFolder(string folderName) =>
      new FileInfo(ThisFilePath()).Directory.EnumerateDirectories(folderName).First().FullName;
    internal static readonly string InlineFolder = GetFolder("Inline");
    internal static readonly string DisplayFolder = GetFolder("Display");
    internal static readonly string TextFolder = GetFolder("Text");
    const float FontSize = 50f;
    const float CanvasWidth = 2000f;
    internal static void Test<TSource>(string inFile, string latex, string folder,
      Rendering.Painter<SKCanvas, TSource, SKColor> painter)
      where TSource: struct, Rendering.ISource {
      painter.FontSize = FontSize;
      painter.LaTeX = latex;
      Assert.Null(painter.ErrorMessage);
      var size = painter.Measure(CanvasWidth) switch {
        System.Drawing.RectangleF rect => rect.Size,
        null => throw new Xunit.Sdk.XunitException("Measure returned null.")
      };
      var actualFile = new FileInfo(Path.Combine(folder, inFile + ".actual.png"));
      using (var surface = SKSurface.Create(new SKImageInfo((int)size.Width, (int)size.Height))) {
        painter.Draw(surface.Canvas, Rendering.TextAlignment.TopLeft);
        using var snapshot = surface.Snapshot();
        using var pngData = snapshot.Encode();
        using var outFile = actualFile.OpenWrite();
        pngData.SaveTo(outFile);
      }
      var expectedFile = new FileInfo(Path.Combine(folder, inFile + ".png"));
      Assert.True(expectedFile.Exists, "The expected image does not exist.");
      Assert.True(actualFile.Exists, "The actual image does not exist.");
      using var actualStream = actualFile.OpenRead();
      using var expectedStream = expectedFile.OpenRead();
      Assert.Equal(expectedStream.Length, actualStream.Length);
      Assert.True(StreamsContentsAreEqual(expectedStream, actualStream));
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
