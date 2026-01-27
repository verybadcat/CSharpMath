namespace CSharpMath.Uno.Example;

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

public sealed partial class TextPage : Page {
  public float[] FontSizes { get; } = TryPage.FontSizes;
  public TextPage() {
    InitializeComponent();
    FontSizer.SelectedItem = 30f;
    // Unlike MAUI and Avalonia, {Binding}s and {x:Bind}s are cleared when a value is assigned.
    // Since LaTeX property automatically re-formats assigned input, we need to bind in the code behind.
    Text.RegisterPropertyChangedCallback(TextBox.TextProperty, (sender, dp) => View.LaTeX = Text.Text);
  }
}