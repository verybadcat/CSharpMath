namespace CSharpMath.Maui.Example {
  public partial class ExamplesPage : ContentPage {
    public ExamplesPage() {
      InitializeComponent();
      Size.ItemsSource = TryPage.FontSizes;
      Size.SelectedItem = View.FontSize;
      Size.SelectedIndexChanged += (sender, e) => View.FontSize = (float)Size.SelectedItem;
      Picker.ItemsSource = Rendering.Tests.TestRenderingMathData.AllConstants.Keys.ToList();
      Picker.SelectedIndexChanged += (sender, e) =>
        View.LaTeX = Label.Text = Rendering.Tests.TestRenderingMathData.AllConstants[(string)Picker.SelectedItem];
      Picker.SelectedItem = nameof(Rendering.Tests.TestRenderingMathData.ShortIntegral);
    }
    protected override void OnDisappearing() {
      base.OnDisappearing();
    }
    private void Button_Clicked(object sender, System.EventArgs e) {
      View.DisplacementX = View.DisplacementY = 0;
    }
  }
}
