namespace CSharpMath.Uno.Example;

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

public sealed partial class TextPage : Page {
  public float[] FontSizes { get; } = TryPage.FontSizes;
  public TextPage() {
    InitializeComponent();
    FontSizer.SelectedItem = 30f;
  }
}