namespace CSharpMath.Utils.NuGet {
  /// <summary>Interaction logic for Editor2.xaml</summary>
  public partial class Editor2 : Avalonia.Controls.Window {
    public Editor2() => InitializeComponent();
    private void InitializeComponent() {
      // TODO: iOS does not support dynamically loading assemblies
      // so we must refer to this resource DLL statically. For
      // now I am doing that here. But we need a better solution!!
      var theme = new Avalonia.Themes.Default.DefaultTheme();
      theme.TryGetResource("Button", out _);
      Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
  }
}