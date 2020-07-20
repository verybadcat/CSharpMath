using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace CSharpMath.Avalonia.Example {
  public class MainView : UserControl {
    public MainView() {
      InitializeComponent();

      var light = AvaloniaXamlLoader.Parse<StyleInclude>(@"<StyleInclude xmlns='https://github.com/avaloniaui' Source='avares://Avalonia.Themes.Fluent/Accents/FluentLight.xaml'/>");
      var dark = AvaloniaXamlLoader.Parse<StyleInclude>(@"<StyleInclude xmlns='https://github.com/avaloniaui' Source='avares://Avalonia.Themes.Fluent/Accents/FluentDark.xaml'/>");
      var themes = this.Find<RadioButton>("lightThemeRbn");
      themes.Checked += (sender, e) => Application.Current.Styles[0] = light;
      themes.Unchecked += (sender, e) => Application.Current.Styles[0] = dark;
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
