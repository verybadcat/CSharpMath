using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CSharpMath.Avalonia.Example {
  public class MainView : UserControl {
    public MainView() {
      InitializeComponent();

      var light = (Styles)AvaloniaXamlLoader.Load(new System.Uri("avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml"));
      var dark = (Styles)AvaloniaXamlLoader.Load(new System.Uri("avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml"));
      var themes = this.Find<RadioButton>("lightThemeRbn");
      themes.Checked += (sender, e) => Application.Current.Styles[0] = light;
      themes.Unchecked += (sender, e) => Application.Current.Styles[0] = dark;
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
