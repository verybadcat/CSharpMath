using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example {
  public class App : Application {
    public override void Initialize() {
      AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted() {
      switch (ApplicationLifetime) {
        case IClassicDesktopStyleApplicationLifetime desktop:
          desktop.MainWindow = new MainWindow();
          break;
        case ISingleViewApplicationLifetime singleView:
          singleView.MainView = new MainView();
          break;
      }
      base.OnFrameworkInitializationCompleted();
    }

    // This method is needed for IDE previewer infrastructure
    public static AppBuilder BuildAvaloniaApp() =>
      AppBuilder.Configure<App>().UsePlatformDetect();

    // The entry point. Things aren't ready yet, so at this point
    // you shouldn't use any Avalonia types or anything that expects
    // a SynchronizationContext to be ready
    public static int Main(string[] args) =>
      BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
  }
}