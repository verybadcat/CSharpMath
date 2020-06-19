using System;
using System.Threading.Tasks;
using Xunit;
using TestData = CSharpMath.Rendering.Tests.TestRenderingMathData;

namespace CSharpMath.Ios.Tests {
  public class Tests {
    /// <summary>Maximum percentage change from expected file size to actual file size * 100</summary>
    /// <remarks>Same idea as CSharpMath.Rendering.Tests.TestRendering.FileSizeTolerance.</remarks>
    const double FileSizeTolerance = 0.95; // This is too large... We need to devise an alternative test mechanism
    static readonly Func<string, System.IO.Stream> GetManifestResourceStream =
      System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream;
    async Task Test(string directory, string file, string latex) {
      var source = new TaskCompletionSource<UIKit.UIImage>();
      Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(() => {
        try {
          using var v = IosMathLabels.MathView(latex, 50);
          var size = v.SizeThatFits(default);
          v.Frame = new CoreGraphics.CGRect(default, size);
          UIKit.UIGraphics.BeginImageContext(size);
          var context = UIKit.UIGraphics.GetCurrentContext();
          context.ScaleCTM(1, -1);
          context.TranslateCTM(0, -size.Height);
          if (!v.DrawViewHierarchy(v.Frame, true))
            throw new Exception(nameof(v.DrawViewHierarchy) + " has failed.");
          source.SetResult(UIKit.UIGraphics.GetImageFromCurrentImageContext());
          UIKit.UIGraphics.EndImageContext();
        } catch (Exception e) {
          source.SetException(e);
        }
      });
      using var data = (await source.Task).AsPNG();
      using var actual = data.AsStream();
      using var expected = GetManifestResourceStream($"CSharpMath.Ios.Tests.{directory}.{file}.png");
      var dir = Foundation.NSSearchPath.GetDirectories(Foundation.NSSearchPathDirectory.DocumentDirectory, Foundation.NSSearchPathDomain.User, true)[0];
      var path = new Foundation.NSUrl(dir).Append($"{directory}.{file}.png", false).Path;
      if (!Foundation.NSFileManager.DefaultManager.CreateFile(path, data, (Foundation.NSDictionary)null))
        throw new System.IO.IOException($"Creation of {path} has failed.");
      switch (file) {
        // The following are produced by inherently different implementations, so they are not comparable
        case nameof(TestData.Cyrillic):
        case nameof(TestData.ErrorInvalidColor):
        case nameof(TestData.ErrorInvalidCommand):
        case nameof(TestData.ErrorMissingBrace):
          break;
        default:
          Assert.InRange(actual.Length, expected.Length * (1 - FileSizeTolerance), expected.Length * (1 + FileSizeTolerance));
          break;
      }
    }
    [Theory]
    [ClassData(typeof(TestData))]
    public Task MathInline(string file, string latex) =>
      Test(nameof(MathInline), file, latex);
    [Theory]
    [ClassData(typeof(TestData))]
    public Task MathDisplay(string file, string latex) =>
      Test(nameof(MathDisplay), file, latex);
  }
}
