using Avalonia;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example {
  public class App : Application {
    public override void Initialize() {
      AvaloniaXamlLoader.Load(this);
    }

    private static void Main() => BuildAvaloniaApp().Start<MainWindow>();

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect();
  }
}
