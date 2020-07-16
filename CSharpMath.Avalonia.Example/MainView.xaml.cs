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
      var themes = this.Find<ComboBox>("Themes");
      themes.SelectionChanged += (sender, e) => {
        switch (themes.SelectedIndex) {
          case 0:
            Application.Current.Styles[0] = light;
            break;
          case 1:
            Application.Current.Styles[0] = dark;
            break;
        }
      };
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
