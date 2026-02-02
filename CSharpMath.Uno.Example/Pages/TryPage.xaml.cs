namespace CSharpMath.Uno.Example;

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

public sealed partial class TryPage : Page {
  public static float[] FontSizes { get; } = [
    1, 2, 4, 8, 12, 16, 20, 24, 30, 36, 48, 60, 72, 96, 108, 144, 192,
    288, 384, 480, 576, 666, 768, 864, 960
  ];
  public static Rendering.FrontEnd.TextAlignment[] AlignmentValues { get; }
  static TryPage() {
    var values = (Rendering.FrontEnd.TextAlignment[])typeof(Rendering.FrontEnd.TextAlignment).GetEnumValues();
    System.Array.Reverse(values);
    AlignmentValues = values;
  }
  public TryPage() {
    InitializeComponent();
    FontSizer.SelectedItem = 30f;
    Alignment.SelectedItem = View.TextAlignment;
    Picker.ItemsSource = Rendering.Tests.TestRenderingMathData.AllConstants.Keys.ToList();
    Picker.SelectionChanged += (sender, e) => {
      if (Picker.SelectedItem is string s) View.LaTeX = Entry.Text = Rendering.Tests.TestRenderingMathData.AllConstants[s];
    };
  }

  private void ResetPan_Clicked(object sender, RoutedEventArgs e) {
    View.DisplacementX = View.DisplacementY = 0;
  }

  private async void Calculate_Clicked(object sender, RoutedEventArgs e) {
    if (View.Painter.Content is not { } content) return;
    await new ContentDialog {
      XamlRoot = XamlRoot,
      Title = "Calculation Result",
      CloseButtonText = "Close",
      Content = new ScrollViewer {
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, // Not visible by default for some reason
        Content = new MathView {
          FontSize = 32,
          TextAlignment = Rendering.FrontEnd.TextAlignment.TopLeft,
          LaTeX = Evaluation.Interpret(content),
          TextColor = View.TextColor
        }
      }
    }.ShowAsync();
  }
}