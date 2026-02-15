using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CSharpMath.Uno.Example;

public sealed partial class MainPage : Page {
  public MainPage() {
    this.InitializeComponent();

    // Handle theme switching
    ThemeSelector.SelectionChanged += (sender, e) => {
      if (ThemeSelector.SelectedItem is ComboBoxItem { Tag: string tag }) {
        // Apply theme to the root content
        if (Window.Current?.Content is FrameworkElement root)
          root.RequestedTheme = tag switch {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
          };
      }
    };

    this.Pivot.SizeChanged += (sender, e) => { // required since WinUI/Uno won't auto constrain PivotItem content width!
      foreach (var pivotItem in Pivot.Items.OfType<PivotItem>())
        pivotItem.MaxWidth = Pivot.ActualWidth - pivotItem.Margin.Left - pivotItem.Margin.Right;
    };
  }
  private async void Benchmark_Open(object sender, RoutedEventArgs e) =>
    await Windows.System.Launcher.LaunchUriAsync(new Uri("https://verybadcat.github.io/CSharpMath/dev/bench"));
}