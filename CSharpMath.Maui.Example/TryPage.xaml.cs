using System.Globalization;

namespace CSharpMath.Maui.Example {
  public partial class TryPage : ContentPage {
    public TryPage() {
      InitializeComponent();
      Size.SelectedItem = View.FontSize;
      Picker.ItemsSource = Rendering.Tests.TestRenderingMathData.AllConstants.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) => {
        if (Picker.SelectedItem is string s) View.LaTeX = Entry.Text = Rendering.Tests.TestRenderingMathData.AllConstants[s];
      };
      var values = typeof(Rendering.FrontEnd.TextAlignment).GetEnumValues();
      Array.Reverse(values);
      Alignment.ItemsSource = values;
      Alignment.SelectedItem = View.TextAlignment;
    }
    private void Button_Clicked(object sender, EventArgs e) =>
      View.DisplacementX = View.DisplacementY = 0;
  }
  public class IsNotNullConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not null;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
  }
}