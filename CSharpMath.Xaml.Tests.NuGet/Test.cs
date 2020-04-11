namespace CSharpMath.Xaml.Tests.NuGet {
  using Avalonia;
  using SkiaSharp;
  using Forms;
  public class Program {
    static string File(string platform, [System.Runtime.CompilerServices.CallerFilePath] string thisDir = "") =>
      System.IO.Path.Combine(thisDir, "..", $"Test.{platform}.png");
    [Xunit.Fact]
    public void TestImage() {
      global::Avalonia.Skia.SkiaPlatform.Initialize();
      Xamarin.Forms.Device.PlatformServices = new Xamarin.Forms.Core.UnitTests.MockPlatformServices();

      using (var forms = System.IO.File.OpenWrite(File(nameof(Forms))))
        new Forms.MathView { LaTeX = "1" }.Painter.DrawAsStream()?.CopyTo(forms);
      using (var avalonia = System.IO.File.OpenWrite(File(nameof(Avalonia))))
        new Avalonia.MathView { LaTeX = "1" }.Painter.DrawAsPng(avalonia);

      using (var forms = System.IO.File.OpenRead(File(nameof(Forms))))
        Xunit.Assert.Contains(forms.Length, new[] { 344L, 797 }); // 797 on Mac, 344 on Ubuntu
      using (var avalonia = System.IO.File.OpenRead(File(nameof(Avalonia))))
        Xunit.Assert.Equal(344, avalonia.Length);
    }
  }
}
