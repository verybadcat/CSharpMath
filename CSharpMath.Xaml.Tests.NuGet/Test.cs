namespace CSharpMath.Xaml.Tests.NuGet {
  using Avalonia;
  public class Program {
    static string File(string platform, [System.Runtime.CompilerServices.CallerFilePath] string thisDir = "") =>
      System.IO.Path.Combine(thisDir, "..", $"Test.{platform}.png");
    [Xunit.Fact]
    public void TestImage() {
      // TODO: old versions of SkiaSharp fail to initialize on arm64 macOS since they only have x64 native libraries
      //global::Avalonia.Skia.SkiaPlatform.Initialize();

      // TODO: Update to use MAUI package
      //using (var forms = System.IO.File.OpenWrite(File(nameof(Forms))))
      //  new Forms.MathView { LaTeX = "1", FontSize = 50 }.Painter.DrawAsStream()?.CopyTo(forms);
      // TODO: Fix Avalonia package issue complaining about missing Avalonia.Visual reference.
      //using (var avalonia = System.IO.File.OpenWrite(File(nameof(Avalonia))))
      //  new Avalonia.MathView { LaTeX = "1", FontSize = 50 }.Painter.DrawAsPng(avalonia);

      var expected = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform
        (System.Runtime.InteropServices.OSPlatform.Windows) ? 344 : 292;
      //using (var forms = System.IO.File.OpenRead(File(nameof(Forms))))
      //  Xunit.Assert.Equal(expected, forms.Length);
      //using (var avalonia = System.IO.File.OpenRead(File(nameof(Avalonia))))
      //  Xunit.Assert.Equal(expected, avalonia.Length);
    }
  }
}