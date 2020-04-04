using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example {
  public class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      Icon = new WindowIcon(System.Reflection.Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("CSharpMath.Avalonia.Example.Icon.png"));
      this.AttachDevTools();
    }

    private void InitializeComponent() {
      // TODO: iOS does not support dynamically loading assemblies
      // so we must refer to this resource DLL statically. For
      // now I am doing that here. But we need a better solution!!
      var theme = new global::Avalonia.Themes.Default.DefaultTheme();
      theme.TryGetResource("Button", out _);
      AvaloniaXamlLoader.Load(this);
    }
  }
}
