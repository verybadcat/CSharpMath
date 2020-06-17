using System;
using System.Threading.Tasks;
using Xunit;
namespace CSharpMath.Ios.Tests {
  public class Tests : IAsyncLifetime {
    Imgur.API.Endpoints.IImageEndpoint endpoint;
    static readonly Func<string, System.IO.Stream> GetManifestResourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream;
    public Task DisposeAsync() => Task.CompletedTask;
    public async Task InitializeAsync() {
      if (ImgurAPI.ImgurClientID is null || ImgurAPI.ImgurClientSecret is null) {
        var source = new TaskCompletionSource<ValueTuple>();
        Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(async () => {
          try {
            var alert = UIKit.UIAlertController.Create($"Imgur Credentials needed", $"Imgur Credentials needed to upload failed tests", UIKit.UIAlertControllerStyle.Alert);
            alert.AddTextField(textField => {
              textField.Placeholder = $"Input Imgur Client ID here...";
              textField.ValueChanged += delegate { ImgurAPI.ImgurClientID = textField.Text; };
            });
            alert.AddTextField(textField => {
              textField.Placeholder = $"Input Imgur Client Secret here...";
              textField.ValueChanged += delegate { ImgurAPI.ImgurClientSecret = textField.Text; };
            });
            alert.AddAction(UIKit.UIAlertAction.Create("OK", UIKit.UIAlertActionStyle.Default, _ => source.SetResult(new ValueTuple())));
            await UIKit.UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewControllerAsync(alert, true);
          } catch (Exception e) { source.SetException(e); }
        });
        await source.Task;
      }
      endpoint ??=
        new Imgur.API.Endpoints.Impl.ImageEndpoint
          (new Imgur.API.Authentication.Impl.ImgurClient
            (ImgurAPI.ImgurClientID, ImgurAPI.ImgurClientSecret));
    }
    async Task Test(string directory, string file, string latex) {
      var v = IosMathLabels.MathView(latex, 50);
      UIKit.UIGraphics.BeginImageContext(v.SizeThatFits(default));
      UIKit.UIApplication.SharedApplication.KeyWindow.DrawViewHierarchy(v.Bounds, true);
      var actual = UIKit.UIGraphics.GetImageFromCurrentImageContext().AsPNG().AsStream();
      UIKit.UIGraphics.EndImageContext();
      var expected = GetManifestResourceStream($"CSharpMath.Ios.Tests.{directory}.{file}.png");
      try {
        Assert.InRange(actual.Length, expected.Length * 0.99, expected.Length * 1.01);
      } catch (Exception e) {
        var image = await endpoint.UploadImageStreamAsync(actual, name: $"Failed test {directory} {file}", description: latex);
        throw new Exception($"Test failed for {directory} {file}: Actual image uploaded at https://imgur.com/gallery/{image.Id}", e);
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
