using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CSharpMath.UWPUno.Example {
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class TryPage : Page {
    public double[] FontSizes = new double[] {
      1, 2, 4, 8, 12, 16, 20, 24, 30, 36, 48, 60, 72, 96, 108, 144, 192,
      288, 384, 480, 576, 666, 768, 864, 960
    };
    public TryPage() {
      InitializeComponent();
      FontSizeComboBox.SelectedItem = View.FontSize;
      FontSizeComboBox.SelectionChanged += (sender, e) =>
        View.FontSize = (double)FontSizeComboBox.SelectedItem;
      Entry.TextChanged += (sender, e) => {
        View.LaTeX = Entry.Text;
        (Exit.Text, Exit.Foreground) =
          (View.LaTeX, View.ErrorMessage != null ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black));
      };
    }
  }
}
