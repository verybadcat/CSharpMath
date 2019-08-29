using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace CSharpMath.Avalonia.Example {
  public class MainView : UserControl {
    public MainView() {
      InitializeComponent();

      var light = AvaloniaXamlLoader.Parse<StyleInclude>(@"<StyleInclude xmlns='https://github.com/avaloniaui' Source='avares://Avalonia.Themes.Default/Accents/BaseLight.xaml'/>");
      var dark = AvaloniaXamlLoader.Parse<StyleInclude>(@"<StyleInclude xmlns='https://github.com/avaloniaui' Source='avares://Avalonia.Themes.Default/Accents/BaseDark.xaml'/>");
      var themes = this.Find<ComboBox>("Themes");
      themes.SelectionChanged += (sender, e) => {
        switch (themes.SelectedIndex) {
          case 0:
            Styles[0] = light;
            break;
          case 1:
            Styles[0] = dark;
            break;
        }
      };

      Styles.Add(light);
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
