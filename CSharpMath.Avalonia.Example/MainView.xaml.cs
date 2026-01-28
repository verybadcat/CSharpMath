using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CSharpMath.Avalonia.Example {
  public class MainView : UserControl {
    public MainView() {
      InitializeComponent();

      var light = this.Find<RadioButton>("lightThemeRbn")!;
      var dark = this.Find<RadioButton>("darkThemeRbn")!;
      if ((string)Application.Current!.ActualThemeVariant.Key == "Dark")
        dark.IsChecked = true;
      else light.IsChecked = true;
      light.IsCheckedChanged += (sender, e) => {
        Application.Current!.RequestedThemeVariant =
          light.IsChecked == true ? ThemeVariant.Light : ThemeVariant.Dark;
      };
      dark.IsCheckedChanged += (sender, e) => {
        Application.Current!.RequestedThemeVariant =
          dark.IsChecked == false ? ThemeVariant.Light : ThemeVariant.Dark;
      };
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}