using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace CSharpMath.Maui.Example {
  public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
      var builder = MauiApp.CreateBuilder();
      builder
          .UseMauiApp<App>()
          .UseMauiCommunityToolkit(options => {
            options.SetPopupDefaults(new() { Margin = default, Padding = default });
          })
          .ConfigureFonts(fonts => {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          });

#if DEBUG
  		builder.Logging.AddDebug();
#endif

      return builder.Build();
    }
  }
}
