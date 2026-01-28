using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example {
  public class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      var stream = System.Reflection.Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("CSharpMath.Avalonia.Example.Icon.png");
      if (stream != null)
        Icon = new WindowIcon(stream);
#if DEBUG
      this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}