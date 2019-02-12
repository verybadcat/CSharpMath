using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CSharpMath.Avalonia.Example.Pages {
  public class TextBlockPage : UserControl {
    public TextBlockPage() {
      InitializeComponent();
    }

    private void InitializeComponent() {
      AvaloniaXamlLoader.Load(this);
    }
  }
}
