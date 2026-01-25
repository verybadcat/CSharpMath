namespace CSharpMath.Uno.Example;

using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

public sealed partial class TryPage : Page {
  public static float[] FontSizes { get; } = [
    1, 2, 4, 8, 12, 16, 20, 24, 30, 36, 48, 60, 72, 96, 108, 144, 192,
    288, 384, 480, 576, 666, 768, 864, 960
  ];

  public Rendering.FrontEnd.TextAlignment[] AlignmentValues { get; }

  public TryPage() {
    var values = (Rendering.FrontEnd.TextAlignment[])typeof(Rendering.FrontEnd.TextAlignment).GetEnumValues();
    System.Array.Reverse(values);
    AlignmentValues = values;
    InitializeComponent();
    Alignment.SelectedItem = View.TextAlignment;
    View.RegisterPropertyChangedCallback(MathView.ErrorMessageProperty, (sender, dp) => {
      Exit.Foreground = View.ErrorMessage is not null
        ? new SolidColorBrush(Colors.Red)
        : (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
    });
  }

  private void ResetPan_Clicked(object sender, RoutedEventArgs e) {
    View.DisplacementX = View.DisplacementY = 0;
  }

  private async void Calculate_Clicked(object sender, RoutedEventArgs e) {
    if (View.Painter.Content is not { } content) return;
    var popupView = new MathView {
      FontSize = 32,
      EnablePanning = true,
      TextAlignment = Rendering.FrontEnd.TextAlignment.TopLeft,
      LaTeX = CSharpMath.Evaluation.Interpret(content),
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
      TextColor = View.TextColor,
      Width = 400,
      Height = 300
    };
    static Button Tap(Button button, Action<object, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs> handler) {
      button.Tapped += (s, e) => handler(s, e);
      return button;
    }
    var dialog = new ContentDialog {
      Title = "Calculation Result",
      Content = new StackPanel {
        Spacing = 10,
        Children = {
          new StackPanel {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = {
              Tap(new Button {
                Content = "Reset pan"
              }, (s, _) => popupView.DisplacementX = popupView.DisplacementY = 0)
            }
          },
          popupView
        }
      },
      CloseButtonText = "Close",
      XamlRoot = this.XamlRoot
    };

    popupView.Loaded += (s, e) => {
      // Force the MathView to render after the dialog is loaded by forcing a measure and invalidate to ensure rendering
      popupView.InvalidateMeasure();
      popupView.Invalidate();
    };

    await dialog.ShowAsync();
  }
}