using Uno.UI.Hosting;

namespace CSharpMath.Uno.Example;

public class Program {
  public static async Task Main(string[] args) {
    App.InitializeLogging();

    var host = UnoPlatformHostBuilder.Create()
        .App(() => new App())
        .UseWebAssembly()
        .Build();

    await host.RunAsync();
  }
}