namespace CSharpMath.Maui.Example {
  public partial class TryPage : ContentPage {
    public static float[] FontSizes = new float[] {
      1, 2, 4, 8, 12, 16, 20, 24, 30, 36, 48, 60, 72, 96, 108, 144, 192,
      288, 384, 480, 576, 666 /*(insert trollface here)*/, 768, 864, 960
    };
    public TryPage() {
      InitializeComponent();
      Size.SelectedItem = View.FontSize;
      Size.SelectedIndexChanged += (sender, e) => View.FontSize = (float)Size.SelectedItem;
      Entry.TextChanged += (sender, e) => {
        View.LaTeX = Entry.Text;
        if (View.ErrorMessage is { })
          Exit.FormattedText = new() { Spans = { new() { Text = View.LaTeX, TextColor = Colors.Red, FontSize = Exit.FontSize } } };
        else Exit.Text = View.LaTeX;
      };
      Picker.ItemsSource = Rendering.Tests.TestRenderingMathData.AllConstants.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) => {
        if (Picker.SelectedItem is string s) View.LaTeX = Entry.Text = Rendering.Tests.TestRenderingMathData.AllConstants[s];
      };
      var values = typeof(Rendering.FrontEnd.TextAlignment).GetEnumValues();
      Array.Reverse(values);
      Alignment.ItemsSource = values;
      Alignment.SelectedIndexChanged += (sender, e) => View.TextAlignment = (Rendering.FrontEnd.TextAlignment)Alignment.SelectedItem;
      Alignment.SelectedItem = View.TextAlignment;
    }
    private void Button_Clicked(object sender, System.EventArgs e) {
      View.DisplacementX = View.DisplacementY = 0;
    }
  }
}
