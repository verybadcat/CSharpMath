using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpMath.Rendering.BackEnd;
using Typography.OpenFont;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  public class TestGlyphFinderConcurrency {
    static Typeface ReadFreshTypeface() {
      var resourceName = typeof(Fonts).Assembly.GetManifestResourceNames()
        .Single(name => name.EndsWith("latinmodern-math.otf", StringComparison.OrdinalIgnoreCase));
      using var stream = typeof(Fonts).Assembly.GetManifestResourceStream(resourceName);
      return new OpenFontReader().Read(stream ?? throw new InvalidOperationException("Embedded math font is missing."))
        ?? throw new InvalidOperationException("Embedded math font is invalid.");
    }

    [Fact]
    public async Task ConcurrentLookupsOnOneTypefaceAreStable() {
      var typeface = ReadFreshTypeface();
      var fonts = new Fonts(new[] { typeface }, 20);
      const int workerCount = 8;
      const int lookupsPerWorker = 256;
      var codepoints = Enumerable.Range('A', workerCount).ToArray();
      var results = new ushort[workerCount * lookupsPerWorker];
      using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
        TestContext.Current.CancellationToken);
      cancellation.CancelAfter(TimeSpan.FromSeconds(5));
      using var start = new Barrier(workerCount + 1);
      var workers = Enumerable.Range(0, workerCount).Select(worker =>
        Task.Factory.StartNew(() => {
          start.SignalAndWait(cancellation.Token);
          var text = char.ConvertFromUtf32(codepoints[worker]);
          for (var i = 0; i < lookupsPerWorker; i++) {
            cancellation.Token.ThrowIfCancellationRequested();
            var glyph = GlyphFinder.Instance.FindGlyphForCharacterAtIndex(fonts, 0, text);
            results[worker * lookupsPerWorker + i] = glyph.Info.GlyphIndex;
          }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
      ).ToArray();
      start.SignalAndWait(cancellation.Token);
      await Task.WhenAll(workers).WaitAsync(cancellation.Token);
      for (var worker = 0; worker < workerCount; worker++) {
        var text = char.ConvertFromUtf32(codepoints[worker]);
        var stable = GlyphFinder.Instance.FindGlyphForCharacterAtIndex(fonts, 0, text).Info.GlyphIndex;
        Assert.NotEqual((ushort)0, stable);
        for (var i = 0; i < lookupsPerWorker; i++) {
          var result = results[worker * lookupsPerWorker + i];
          Assert.NotEqual((ushort)0, result);
          Assert.Equal(stable, result);
        }
      }
    }
  }
}
