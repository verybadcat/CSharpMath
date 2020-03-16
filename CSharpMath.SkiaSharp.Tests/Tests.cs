using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CSharpMath.SkiaSharp
{
    using Rendering.Renderer;
    public class TestsFixture : IDisposable
    {
        // Switch this on and run all tests if you are curious :)
        public static bool TestIfAllImagesWereTested = false;
        public TestsFixture()
        {
            // Pre-initialize the typefaces to speed tests up
            Rendering.FrontEnd.Fonts.GlobalTypefaces.ToString();
            // Delete garbage by previous tests
            foreach (var garbage in
              Tests.Folders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.actual.*")))
                File.Delete(garbage);
        }
        public void Dispose()
        {
            Assert.NotEmpty(Tests.Folders);
            if (System.Diagnostics.Debugger.IsAttached || !TestIfAllImagesWereTested)
                return;
            // Verify that all expected images have been tested against
            // FAILS if tests are executed in isolation!! (Not all tests are run)
            Assert.All(
              Tests.Folders.SelectMany(folder =>
                Directory.EnumerateFiles(folder)
                .Select(file => Path.GetRelativePath(Tests.ThisDirectory.FullName, file))
                .GroupBy(path => path.Split('.')[0])
              ),
              file => Assert.Collection(file,
                f => Assert.EndsWith(".actual.png", f),
                f => Assert.EndsWith(".png", f)
              )
            );
        }
    }
    public class Tests : IClassFixture<TestsFixture>
    {
        // https://www.codecogs.com/latex/eqneditor.php
        static string ThisFilePath
          ([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;
        public static DirectoryInfo ThisDirectory = new FileInfo(ThisFilePath()).Directory;
        public static string GetFolder(string folderName) =>
          ThisDirectory.CreateSubdirectory(folderName).FullName;
        public static IEnumerable<string> Folders =>
          typeof(Tests)
          .GetMethods()
          .Where(method => method.IsDefined(typeof(TheoryAttribute), false))
          .Select(method => GetFolder(method.Name));

        [Theory, ClassData(typeof(MathData))]
        public void Display(string file, string latex) =>
          Test(file, latex, GetFolder(nameof(Display)), new MathPainter { LineStyle = Atom.LineStyle.Display });
        [Theory, ClassData(typeof(MathData))]
        public void Inline(string file, string latex) =>
          Test(file, latex, GetFolder(nameof(Inline)), new MathPainter { LineStyle = Atom.LineStyle.Text });
        [Theory, ClassData(typeof(TextData))]
        public void Text(string file, string latex) =>
          Test(file, latex, GetFolder(nameof(Text)), new TextPainter());

        static void Test<TSource>(string inFile, string latex, string folder,
          Painter<SKCanvas, TSource, SKColor> painter) where TSource : ISource
        {
            // Prevent black background behind black rendered output in File Explorer preview
            painter.HighlightColor = new SKColor(0xF0, 0xF0, 0xF0);
            painter.FontSize = 50f;
            painter.LaTeX = latex;
            if (painter.ErrorMessage != null)
                throw new Xunit.Sdk.XunitException("Painter error: " + painter.ErrorMessage);

            var size = painter.Measure(2000f) switch
            {
                System.Drawing.RectangleF rect => rect.Size,
                null => throw new Xunit.Sdk.XunitException("Measure returned null.")
            };
            var actualFile = new FileInfo(Path.Combine(folder, inFile + ".actual.png"));
            Assert.False(actualFile.Exists, "The actual file was not deleted by test initialization.");
            using (var surface = SKSurface.Create(new SKImageInfo((int)size.Width, (int)size.Height)))
            {
                painter.Draw(surface.Canvas, Rendering.Renderer.TextAlignment.TopLeft);
                using var snapshot = surface.Snapshot();
                using var pngData = snapshot.Encode();
                using var outFile = actualFile.OpenWrite();
                pngData.SaveTo(outFile);
            }
            actualFile.Refresh();
            Assert.True(actualFile.Exists, "The actual image was not created successfully.");

            var expectedFile = new FileInfo(Path.Combine(folder, inFile + ".png"));
            if (!expectedFile.Exists)
            {
                actualFile.CopyTo(expectedFile.FullName);
                expectedFile.Refresh();
            }
            Assert.True(expectedFile.Exists, "The expected image was not copied successfully.");
            using var actualStream = actualFile.OpenRead();
            using var expectedStream = expectedFile.OpenRead();
            Assert.Equal(expectedStream.Length, actualStream.Length);
            Assert.True(StreamsContentsAreEqual(expectedStream, actualStream), "The images differ.");

            // https://stackoverflow.com/a/2637303/5429648
            static bool StreamsContentsAreEqual(Stream stream1, Stream stream2)
            {
                const int bufferSize = 2048 * 2;
                var buffer1 = new byte[bufferSize];
                var buffer2 = new byte[bufferSize];

                while (true)
                {
                    int count1 = stream1.Read(buffer1, 0, bufferSize);
                    int count2 = stream2.Read(buffer2, 0, bufferSize);

                    if (count1 != count2)
                    {
                        return false;
                    }

                    if (count1 == 0)
                    {
                        return true;
                    }

                    int iterations = (int)Math.Ceiling((double)count1 / sizeof(long));
                    for (int i = 0; i < iterations; i++)
                    {
                        if (BitConverter.ToInt64(buffer1, i * sizeof(long)) != BitConverter.ToInt64(buffer2, i * sizeof(long)))
                        {
                            return false;
                        }
                    }
                }
            }
        }
    }
}
