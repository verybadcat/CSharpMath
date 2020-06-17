using System;
using System.Threading.Tasks;
using Xunit;

namespace CSharpMath.Ios.Tests {
  public class Tests {
    static readonly Imgur.API.Endpoints.IImageEndpoint endpoint = null;
    static readonly Func<string, System.IO.Stream> GetManifestResourceStream =
      System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream;
    static Tests() {
      if (ImgurAPI.ImgurCredentials is var (id, secret)) 
        endpoint ??=
          new Imgur.API.Endpoints.Impl.ImageEndpoint(new Imgur.API.Authentication.Impl.ImgurClient(id, secret));
      else
        Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(async () => {
          using var alert = UIKit.UIAlertController.Create($"Imgur Credentials not provided", $"Imgur Credentials are not provided in ImgurAPI.cs. Uploading of failed images are disabled.", UIKit.UIAlertControllerStyle.Alert);
          alert.AddAction(UIKit.UIAlertAction.Create("OK", UIKit.UIAlertActionStyle.Default, null));
          await UIKit.UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewControllerAsync(alert, true);
        });
    }
    async Task Test(string directory, string file, string latex) {
      var source = new TaskCompletionSource<UIKit.UIImage>();
      Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(() => {
        try {
          using var v = IosMathLabels.MathView(latex, 50);
          UIKit.UIApplication.SharedApplication.KeyWindow.AddSubview(v);
          UIKit.UIGraphics.BeginImageContext(v.SizeThatFits(default));
          UIKit.UIApplication.SharedApplication.KeyWindow.DrawViewHierarchy(v.Bounds, true);
          source.SetResult(UIKit.UIGraphics.GetImageFromCurrentImageContext());
          UIKit.UIGraphics.EndImageContext();
          v.RemoveFromSuperview();
        } catch (Exception e) {
          source.SetException(e);
        }
      });
      using var actual = (await source.Task).AsPNG().AsStream();
      using var expected = GetManifestResourceStream($"CSharpMath.Ios.Tests.{directory}.{file}.png");
      try {
        Assert.InRange(actual.Length, expected.Length * 0.99, expected.Length * 1.01);
      } catch (Exception e) when (endpoint is { }) {
        var image = await endpoint.UploadImageStreamAsync(actual, name: $"Failed test {directory} {file}", description: latex);
        throw new Exception($"Test failed for {directory} {file}: Actual image uploaded at https://imgur.com/{image.Id}", e);
      }
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
