using Microsoft.UI.Xaml.Controls;

namespace CSharpMath.Uno.Example;

public sealed partial class MainPage : Page {
  public MainPage() {
    this.InitializeComponent();
    this.Pivot.SizeChanged += (sender, e) => { // required since WinUI/Uno won't auto constrain PivotItem content width!
      foreach (var pivotItem in Pivot.Items.OfType<PivotItem>())
        if (pivotItem.Content is FrameworkElement pivotItemContent)
          pivotItemContent.MaxWidth = Pivot.ActualWidth - pivotItem.Margin.Left - pivotItem.Margin.Right;
    };
  }
}