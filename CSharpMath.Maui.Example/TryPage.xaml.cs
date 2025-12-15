namespace CSharpMath.Maui.Example {
  public partial class TryPage : ContentPage {
    public static float[] FontSizes = new float[] {
      1, 2, 4, 8, 12, 14, 16, 20, 24, 30, 36, 48, 60, 72, 96, 108, 144, 192,
      288, 384, 480, 576, 666 /*(insert trollface here)*/, 768, 864, 960
    };
    public TryPage() {
      InitializeComponent();
      //App.AllMathViews.Add(View);
      Size.SelectedItem = View.FontSize;
      Size.SelectedIndexChanged += (sender, e) =>
        View.FontSize = (float)Size.SelectedItem;
      Entry.TextChanged += (sender, e) => {
        View.LaTeX = Entry.Text;
        (Exit.Text, Exit.TextColor) =
          (View.LaTeX, View.ErrorMessage != null ? Colors.Red : (Color)Application.Current!.Resources["Primary"]);
      };
    }
    private void Button_Clicked(object sender, System.EventArgs e) {
      View.DisplacementX = View.DisplacementY = 0;
    }
  }
}
