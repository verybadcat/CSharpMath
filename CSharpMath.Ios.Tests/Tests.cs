using System;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Ios.Tests {
  public class Tests {
    static readonly Func<string, System.IO.Stream> GetManifestResourceStream =
      System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream;
    async Task Test(string directory, string file, string latex) {
      var source = new TaskCompletionSource<UIKit.UIImage>();
      Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(() => {
        try {
          using var v = IosMathLabels.MathView(latex, 50);
          var window = UIKit.UIApplication.SharedApplication.KeyWindow;
          window.AddSubview(v);
          UIKit.UIGraphics.BeginImageContext(new CoreGraphics.CGSize(1000, 1000));
          if (!window.DrawViewHierarchy(new CoreGraphics.CGRect(0, 0, 1000, 1000), true))
            throw new Exception(nameof(window.DrawViewHierarchy) + " has failed.");
          source.SetResult(UIKit.UIGraphics.GetImageFromCurrentImageContext());
          UIKit.UIGraphics.EndImageContext();
          v.RemoveFromSuperview();
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
      Assert.InRange(actual.Length, expected.Length * 0.99, expected.Length * 1.01);
    }
    [Theory]
    [ClassData(typeof(Rendering.Tests.TestRenderingMathData))]
    public Task MathInline(string file, string latex) =>
      Test(nameof(MathInline), file, latex);
    [Theory]
    [ClassData(typeof(Rendering.Tests.TestRenderingMathData))]
    public Task MathDisplay(string file, string latex) =>
      Test(nameof(MathDisplay), file, latex);
  }
}
